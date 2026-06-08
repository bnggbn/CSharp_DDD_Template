using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class RoslynRuleRunner
{
    public static void Run(LinterPolicy policy, string repoRoot, List<string> files, List<string> issues)
    {
        SemanticCompilationContext semanticContext = SemanticCompilationContext.Create(repoRoot, files);
        CheckDependencyRules(policy, files, issues, semanticContext);
        CheckUseCaseFileRules(policy, files, issues);
        CheckCqrsCommandFileRules(policy, files, issues);
        CheckCqrsQueryFileRules(policy, files, issues);
        CheckRequestImmutabilityRule(policy, files, issues);
        CheckCqrsInheritanceRule(policy, files, issues);
        CheckWorkflowConstructorRule(policy, files, issues, semanticContext);
        CheckWorkflowDispatchRule(policy, files, issues, semanticContext);
        CheckHandlerDispatchRule(policy, files, issues, semanticContext);
        CheckControllerWorkflowRule(policy, files, issues, semanticContext);
        CheckSeverityMutationRule(policy, files, issues);
        CheckConstructorInterfaceRule(policy, files, issues, semanticContext);
        CheckConstantsClassRule(policy, files, issues);
    }

    private static void CheckDependencyRules(LinterPolicy policy, List<string> files, List<string> issues, SemanticCompilationContext semanticContext)
    {
        foreach (LayerRule rule in policy.DependencyRules)
        {
            foreach (string file in files)
            {
                string rel = Normalize(file);
                if (!PathMatchesScope(rel, rule.PathContains))
                {
                    continue;
                }

                CompilationUnitSyntax root = (CompilationUnitSyntax)semanticContext.GetSyntaxTree(file).GetRoot();
                SemanticModel semanticModel = semanticContext.GetSemanticModel(file);
                HashSet<string> forbiddenHits = new(StringComparer.Ordinal);

                foreach (UsingDirectiveSyntax usingDirective in root.Usings)
                {
                    if (usingDirective.Name != null)
                    {
                        AddForbiddenNamespaceHit(forbiddenHits, semanticModel.GetSymbolInfo(usingDirective.Name).Symbol, rule.ForbiddenNamespaces);
                    }
                }

                foreach (IdentifierNameSyntax identifier in root.DescendantNodes().OfType<IdentifierNameSyntax>())
                {
                    AddForbiddenNamespaceHit(forbiddenHits, semanticModel.GetSymbolInfo(identifier).Symbol, rule.ForbiddenNamespaces);
                }

                foreach (GenericNameSyntax generic in root.DescendantNodes().OfType<GenericNameSyntax>())
                {
                    AddForbiddenNamespaceHit(forbiddenHits, semanticModel.GetSymbolInfo(generic).Symbol, rule.ForbiddenNamespaces);
                }

                foreach (ObjectCreationExpressionSyntax creation in root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
                {
                    AddForbiddenNamespaceHit(forbiddenHits, semanticModel.GetSymbolInfo(creation).Symbol, rule.ForbiddenNamespaces);
                    if (creation.Type != null)
                    {
                        AddForbiddenNamespaceHit(forbiddenHits, semanticModel.GetTypeInfo(creation.Type).Type, rule.ForbiddenNamespaces);
                    }
                }

                foreach (BaseTypeSyntax baseType in root.DescendantNodes().OfType<BaseTypeSyntax>())
                {
                    AddForbiddenNamespaceHit(forbiddenHits, semanticModel.GetTypeInfo(baseType.Type).Type, rule.ForbiddenNamespaces);
                }

                foreach (string forbiddenNamespace in forbiddenHits.OrderBy(static value => value, StringComparer.Ordinal))
                {
                    issues.Add($"[{rule.RuleId}] {rel}: must not depend on '{forbiddenNamespace}'.");
                }
            }
        }
    }

    private static void CheckUseCaseFileRules(LinterPolicy policy, List<string> files, List<string> issues)
    {
        if (!policy.UseCaseFileRule.Enabled)
        {
            return;
        }

        foreach (string file in files)
        {
            string rel = Normalize(file);
            if (!PathMatchesScope(rel, policy.UseCaseFileRule.UseCasesPathContains))
            {
                continue;
            }

            string fileName = Path.GetFileName(file);
            if (!fileName.EndsWith("UseCase.cs", StringComparison.Ordinal))
            {
                issues.Add($"[{policy.UseCaseFileRule.RuleId}] {rel}: file name must end with 'UseCase.cs'.");
            }

            string text = File.ReadAllText(file);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(text);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            List<ClassDeclarationSyntax> classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
            if (classes.Count == 0)
            {
                issues.Add($"[{policy.UseCaseFileRule.RuleId}] {rel}: use-case file must declare a '*UseCase' class.");
                continue;
            }

            foreach (ClassDeclarationSyntax cls in classes)
            {
                if (!cls.Identifier.Text.EndsWith("UseCase", StringComparison.Ordinal))
                {
                    issues.Add($"[{policy.UseCaseFileRule.RuleId}] {rel}: class '{cls.Identifier.Text}' must end with 'UseCase'.");
                }

                foreach (MemberDeclarationSyntax member in cls.Members)
                {
                    if (member is not RecordDeclarationSyntax record)
                    {
                        issues.Add($"[{policy.UseCaseFileRule.RuleId}] {rel}: class '{cls.Identifier.Text}' may only contain record command/query definitions.");
                        continue;
                    }

                    string recordName = record.Identifier.Text;
                    if (!recordName.EndsWith("Command", StringComparison.Ordinal) &&
                        !recordName.EndsWith("Query", StringComparison.Ordinal))
                    {
                        issues.Add($"[{policy.UseCaseFileRule.RuleId}] {rel}: record '{recordName}' must end with 'Command' or 'Query'.");
                    }
                }
            }
        }
    }

    private static void CheckCqrsCommandFileRules(LinterPolicy policy, List<string> files, List<string> issues)
    {
        ValidateRequestFileRule(files, issues, policy.CqrsCommandFileRule.RuleId, policy.CqrsCommandFileRule.CommandsPathContains, "Command", requireBusinessUseCaseContainer: true);
    }

    private static void CheckCqrsQueryFileRules(LinterPolicy policy, List<string> files, List<string> issues)
    {
        ValidateRequestFileRule(files, issues, policy.CqrsQueryFileRule.RuleId, policy.CqrsQueryFileRule.QueriesPathContains, "Query", requireBusinessUseCaseContainer: true);
    }

    private static void ValidateRequestFileRule(List<string> files, List<string> issues, string ruleId, string pathContains, string requestSuffix, bool requireBusinessUseCaseContainer)
    {
        foreach (string file in files)
        {
            string rel = Normalize(file);
            CompilationUnitSyntax root = ParseRoot(file);

            List<TypeDeclarationSyntax> types = root.DescendantNodes().OfType<TypeDeclarationSyntax>().ToList();
            List<TypeDeclarationSyntax> requestTypes = types
                .Where(x => x.Identifier.Text.EndsWith(requestSuffix, StringComparison.Ordinal))
                .ToList();

            if (requestTypes.Count == 0)
            {
                continue;
            }

            foreach (TypeDeclarationSyntax requestType in requestTypes)
            {
                if (requestType is not RecordDeclarationSyntax)
                {
                    issues.Add($"[{ruleId}] {rel}: '{requestType.Identifier.Text}' must be declared as record.");
                }

                if (!PathMatchesScope(rel, pathContains))
                {
                    issues.Add($"[{ruleId}] {rel}: '{requestType.Identifier.Text}' must be declared under '{pathContains}'.");
                }

                if (requireBusinessUseCaseContainer)
                {
                    ClassDeclarationSyntax? parentClass = requestType.Parent as ClassDeclarationSyntax;
                    bool isInsideBusinessUseCase =
                        parentClass != null &&
                        parentClass.Identifier.Text.EndsWith("BusinessUseCase", StringComparison.Ordinal);

                    if (!isInsideBusinessUseCase)
                    {
                        issues.Add($"[{ruleId}] {rel}: '{requestType.Identifier.Text}' must be nested inside '*BusinessUseCase'.");
                    }
                }
            }
        }
    }

    private static void CheckSeverityMutationRule(LinterPolicy policy, List<string> files, List<string> issues)
    {
        string methodName = policy.SeverityMutationRule.TargetMethodName;
        foreach (string file in files)
        {
            string rel = Normalize(file);
            if (policy.SeverityMutationRule.AllowedPathContains.Any(path => rel.Contains(path, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            string text = File.ReadAllText(file);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(text);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            bool hasForbiddenCall = root
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any(invocation => invocation.Expression.ToString().EndsWith("." + methodName, StringComparison.Ordinal));

            if (hasForbiddenCall)
            {
                issues.Add($"[{policy.SeverityMutationRule.RuleId}] {rel}: '{methodName}' may only be called in approved paths.");
            }
        }
    }

    private static void CheckRequestImmutabilityRule(LinterPolicy policy, List<string> files, List<string> issues)
    {
        if (!policy.RequestImmutabilityRule.Enabled)
        {
            return;
        }

        foreach (string file in files)
        {
            string rel = Normalize(file);
            CompilationUnitSyntax root = ParseRoot(file);
            foreach (RecordDeclarationSyntax record in root.DescendantNodes().OfType<RecordDeclarationSyntax>())
            {
                if (!IsRequestRecord(record))
                {
                    continue;
                }

                if (!PathMatchesScope(rel, policy.RequestImmutabilityRule.RequestsPathContains))
                {
                    issues.Add($"[{policy.RequestImmutabilityRule.RuleId}] {rel}: request record '{record.Identifier.Text}' must be declared under '{policy.RequestImmutabilityRule.RequestsPathContains}'.");
                }

                foreach (PropertyDeclarationSyntax property in record.Members.OfType<PropertyDeclarationSyntax>())
                {
                    AccessorDeclarationSyntax? setter = property.AccessorList?.Accessors
                        .FirstOrDefault(accessor => accessor.IsKind(SyntaxKind.SetAccessorDeclaration));
                    if (setter != null)
                    {
                        issues.Add($"[{policy.RequestImmutabilityRule.RuleId}] {rel}: request record '{record.Identifier.Text}' property '{property.Identifier.Text}' must be init-only.");
                    }
                }

                foreach (FieldDeclarationSyntax field in record.Members.OfType<FieldDeclarationSyntax>())
                {
                    bool isReadonly = field.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.ReadOnlyKeyword));
                    bool isConst = field.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.ConstKeyword));
                    if (!isReadonly && !isConst)
                    {
                        issues.Add($"[{policy.RequestImmutabilityRule.RuleId}] {rel}: request record '{record.Identifier.Text}' field '{GetFieldName(field)}' must be readonly.");
                    }
                }
            }
        }
    }

    private static void CheckCqrsInheritanceRule(LinterPolicy policy, List<string> files, List<string> issues)
    {
        if (!policy.CqrsInheritanceRule.Enabled)
        {
            return;
        }

        HashSet<string> allowedTypeNames = new(policy.CqrsInheritanceRule.AllowedInterfaceTypeNames, StringComparer.Ordinal);
        foreach (string file in files)
        {
            string rel = Normalize(file);
            CompilationUnitSyntax root = ParseRoot(file);
            foreach (RecordDeclarationSyntax record in root.DescendantNodes().OfType<RecordDeclarationSyntax>())
            {
                if (!IsRequestRecord(record))
                {
                    continue;
                }

                if (!PathMatchesScope(rel, policy.CqrsInheritanceRule.RequestsPathContains))
                {
                    issues.Add($"[{policy.CqrsInheritanceRule.RuleId}] {rel}: request record '{record.Identifier.Text}' must be declared under '{policy.CqrsInheritanceRule.RequestsPathContains}'.");
                }

                foreach (BaseTypeSyntax baseType in record.BaseList?.Types ?? Enumerable.Empty<BaseTypeSyntax>())
                {
                    string terminalName = GetTerminalTypeName(baseType.Type);
                    if (!allowedTypeNames.Contains(terminalName))
                    {
                        issues.Add($"[{policy.CqrsInheritanceRule.RuleId}] {rel}: request record '{record.Identifier.Text}' must not inherit or implement '{terminalName}'.");
                    }
                }
            }
        }
    }

    private static void CheckWorkflowConstructorRule(LinterPolicy policy, List<string> files, List<string> issues, SemanticCompilationContext semanticContext)
    {
        if (!policy.WorkflowConstructorRule.Enabled)
        {
            return;
        }

        foreach (string file in files)
        {
            string rel = Normalize(file);
            if (!PathMatchesScope(rel, policy.WorkflowConstructorRule.WorkflowsPathContains))
            {
                continue;
            }

            CompilationUnitSyntax root = (CompilationUnitSyntax)semanticContext.GetSyntaxTree(file).GetRoot();
            SemanticModel semanticModel = semanticContext.GetSemanticModel(file);
            foreach (ConstructorDeclarationSyntax ctor in root.DescendantNodes().OfType<ConstructorDeclarationSyntax>())
            {
                foreach (ParameterSyntax parameter in ctor.ParameterList.Parameters)
                {
                    if (parameter.Type == null)
                    {
                        continue;
                    }

                    ITypeSymbol? parameterType = semanticModel.GetTypeInfo(parameter.Type).Type;
                    if (parameterType == null)
                    {
                        continue;
                    }

                    if (!IsAllowedWorkflowDependency(parameterType, policy.WorkflowConstructorRule.AllowedDependencyTypeNames))
                    {
                        issues.Add($"[{policy.WorkflowConstructorRule.RuleId}] {rel}: workflow constructor dependency '{parameterType.ToDisplayString()}' is not allowed. Depend on 'MediatR.ISender' only.");
                    }
                }
            }
        }
    }

    private static void CheckWorkflowDispatchRule(LinterPolicy policy, List<string> files, List<string> issues, SemanticCompilationContext semanticContext)
    {
        if (!policy.WorkflowDispatchRule.Enabled)
        {
            return;
        }

        foreach (string file in files)
        {
            string rel = Normalize(file);
            if (!PathMatchesScope(rel, policy.WorkflowDispatchRule.WorkflowsPathContains))
            {
                continue;
            }

            CompilationUnitSyntax root = (CompilationUnitSyntax)semanticContext.GetSyntaxTree(file).GetRoot();
            SemanticModel semanticModel = semanticContext.GetSemanticModel(file);
            INamedTypeSymbol? containingWorkflowType = root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Select(classDeclaration => semanticModel.GetDeclaredSymbol(classDeclaration))
                .OfType<INamedTypeSymbol>()
                .FirstOrDefault();
            foreach (MethodDeclarationSyntax method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                foreach (InvocationExpressionSyntax invocation in method.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    if (!TryGetInjectedDependencyTarget(semanticModel, containingWorkflowType, invocation, out ISymbol? receiverSymbol, out IMethodSymbol? targetMethodSymbol))
                    {
                        continue;
                    }

                    if (!IsAllowedWorkflowDispatchCall(receiverSymbol, targetMethodSymbol))
                    {
                        issues.Add($"[{policy.WorkflowDispatchRule.RuleId}] {rel}: workflow method '{method.Identifier.Text}' must not call injected dependency '{receiverSymbol?.Name}.{targetMethodSymbol?.Name}(...)'. Use 'ISender.Send(new <Request>(...), ct)' for orchestration.");
                    }
                }
            }
        }
    }

    private static void CheckHandlerDispatchRule(LinterPolicy policy, List<string> files, List<string> issues, SemanticCompilationContext semanticContext)
    {
        if (!policy.HandlerDispatchRule.Enabled)
        {
            return;
        }

        foreach (string file in files)
        {
            string rel = Normalize(file);
            if (!PathMatchesScope(rel, policy.HandlerDispatchRule.HandlersPathContains))
            {
                continue;
            }

            CompilationUnitSyntax root = (CompilationUnitSyntax)semanticContext.GetSyntaxTree(file).GetRoot();
            SemanticModel semanticModel = semanticContext.GetSemanticModel(file);
            foreach (ConstructorDeclarationSyntax ctor in root.DescendantNodes().OfType<ConstructorDeclarationSyntax>())
            {
                foreach (ParameterSyntax parameter in ctor.ParameterList.Parameters)
                {
                    if (parameter.Type == null)
                    {
                        continue;
                    }

                    ITypeSymbol? parameterType = semanticModel.GetTypeInfo(parameter.Type).Type;
                    if (parameterType != null && IsForbiddenMediatorType(parameterType, policy.HandlerDispatchRule.ForbiddenDependencyTypeNames))
                    {
                        issues.Add($"[{policy.HandlerDispatchRule.RuleId}] {rel}: handler constructor must not depend on '{parameterType.ToDisplayString()}'. Keep orchestration out of handlers.");
                    }
                }
            }

            foreach (InvocationExpressionSyntax invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                IMethodSymbol? methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                if (methodSymbol != null && IsForbiddenMediatorInvocation(methodSymbol, policy.HandlerDispatchRule.ForbiddenInvocationNames))
                {
                    issues.Add($"[{policy.HandlerDispatchRule.RuleId}] {rel}: handler must not orchestrate via '{methodSymbol.ContainingType.ToDisplayString()}.{methodSymbol.Name}(...)'.");
                }
            }
        }
    }

    private static void CheckControllerWorkflowRule(LinterPolicy policy, List<string> files, List<string> issues, SemanticCompilationContext semanticContext)
    {
        if (!policy.ControllerWorkflowRule.Enabled)
        {
            return;
        }

        foreach (string file in files)
        {
            string rel = Normalize(file);
            if (!PathMatchesScope(rel, policy.ControllerWorkflowRule.ControllersPathContains))
            {
                continue;
            }

            CompilationUnitSyntax root = (CompilationUnitSyntax)semanticContext.GetSyntaxTree(file).GetRoot();
            SemanticModel semanticModel = semanticContext.GetSemanticModel(file);
            foreach (ConstructorDeclarationSyntax ctor in root.DescendantNodes().OfType<ConstructorDeclarationSyntax>())
            {
                foreach (ParameterSyntax parameter in ctor.ParameterList.Parameters)
                {
                    if (parameter.Type == null)
                    {
                        continue;
                    }

                    ITypeSymbol? parameterType = semanticModel.GetTypeInfo(parameter.Type).Type;
                    if (parameterType == null || IsAllowedNonInterfaceType(parameterType.Name))
                    {
                        continue;
                    }

                    if (!IsAllowedControllerDependency(parameterType, policy))
                    {
                        issues.Add($"[{policy.ControllerWorkflowRule.RuleId}] {rel}: controller constructor dependency '{parameterType.ToDisplayString()}' must stay in workflow/framework scope.");
                    }
                }
            }
        }
    }

    private static void CheckConstructorInterfaceRule(LinterPolicy policy, List<string> files, List<string> issues, SemanticCompilationContext semanticContext)
    {
        if (!policy.ConstructorInterfaceRule.Enabled)
        {
            return;
        }

        foreach (string file in files)
        {
            string rel = Normalize(file);
            if (!policy.ConstructorInterfaceRule.TargetPathContains.Any(path => PathMatchesScope(rel, path)))
            {
                continue;
            }

            CompilationUnitSyntax root = (CompilationUnitSyntax)semanticContext.GetSyntaxTree(file).GetRoot();
            SemanticModel semanticModel = semanticContext.GetSemanticModel(file);

            foreach (ConstructorDeclarationSyntax ctor in root.DescendantNodes().OfType<ConstructorDeclarationSyntax>())
            {
                foreach (ParameterSyntax parameter in ctor.ParameterList.Parameters)
                {
                    if (parameter.Type == null)
                    {
                        continue;
                    }

                    ITypeSymbol? parameterType = semanticModel.GetTypeInfo(parameter.Type).Type;
                    if (parameterType == null || IsAllowedConstructorDependency(parameterType))
                    {
                        continue;
                    }

                    if (parameterType.TypeKind != TypeKind.Interface)
                    {
                        issues.Add($"[{policy.ConstructorInterfaceRule.RuleId}] {rel}: constructor dependency '{parameterType.ToDisplayString()}' should depend on interface.");
                    }
                }
            }
        }
    }

    private static string GetTerminalTypeName(TypeSyntax? typeSyntax)
    {
        if (typeSyntax == null)
        {
            return string.Empty;
        }

        return typeSyntax switch
        {
            IdentifierNameSyntax id => id.Identifier.Text,
            GenericNameSyntax generic => generic.Identifier.Text,
            QualifiedNameSyntax qualified => GetTerminalTypeName(qualified.Right),
            AliasQualifiedNameSyntax aliasQualified => GetTerminalTypeName(aliasQualified.Name),
            NullableTypeSyntax nullable => GetTerminalTypeName(nullable.ElementType),
            PredefinedTypeSyntax predefined => predefined.Keyword.Text,
            _ => typeSyntax.ToString().Split('.').Last()
        };
    }

    private static CompilationUnitSyntax ParseRoot(string file)
    {
        string text = File.ReadAllText(file);
        SyntaxTree tree = CSharpSyntaxTree.ParseText(text);
        return tree.GetCompilationUnitRoot();
    }

    private static bool IsRequestRecord(RecordDeclarationSyntax record)
    {
        string recordName = record.Identifier.Text;
        return recordName.EndsWith("Command", StringComparison.Ordinal) ||
               recordName.EndsWith("Query", StringComparison.Ordinal);
    }

    private static string GetInvocationName(InvocationExpressionSyntax invocation)
    {
        return invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
            IdentifierNameSyntax identifier => identifier.Identifier.Text,
            GenericNameSyntax generic => generic.Identifier.Text,
            _ => invocation.Expression.ToString().Split('.').Last()
        };
    }

    private static bool IsAllowedNonInterfaceType(string typeName)
    {
        return typeName switch
        {
            "string" or "int" or "long" or "bool" or "double" or "float" or "decimal" or
            "byte" or "short" or "uint" or "ulong" or "ushort" or "sbyte" or "char" or
            "Guid" or "DateTime" or "DateTimeOffset" or "TimeSpan" or
            "CancellationToken" or "IServiceProvider" => true,
            _ => false
        };
    }

    private static void CheckConstantsClassRule(LinterPolicy policy, List<string> files, List<string> issues)
    {
        if (!policy.ConstantsClassRule.Enabled)
        {
            return;
        }

        foreach (string file in files)
        {
            string rel = Normalize(file);
            if (!policy.ConstantsClassRule.TargetPathContains.Any(path => PathMatchesScope(rel, path)))
            {
                continue;
            }

            string text = File.ReadAllText(file);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(text);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            IEnumerable<ClassDeclarationSyntax> constantsClasses = root
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Where(cls => cls.Identifier.Text.EndsWith(policy.ConstantsClassRule.ClassSuffix, StringComparison.Ordinal));

            foreach (ClassDeclarationSyntax constantsClass in constantsClasses)
            {
                foreach (MemberDeclarationSyntax member in constantsClass.Members)
                {
                    if (member is FieldDeclarationSyntax field)
                    {
                        bool isConst = field.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword));
                        bool isStatic = field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
                        bool isReadonly = field.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword));
                        if (isConst || (isStatic && isReadonly))
                        {
                            continue;
                        }

                        issues.Add($"[{policy.ConstantsClassRule.RuleId}] {rel}: '{constantsClass.Identifier.Text}' field '{GetFieldName(field)}' must be const or static readonly.");
                        continue;
                    }

                    issues.Add($"[{policy.ConstantsClassRule.RuleId}] {rel}: '{constantsClass.Identifier.Text}' contains unsupported member '{member.Kind()}'.");
                }
            }
        }
    }

    private static string GetFieldName(FieldDeclarationSyntax field)
    {
        return field.Declaration.Variables.FirstOrDefault()?.Identifier.Text ?? "(unknown)";
    }

    private static string Normalize(string path)
    {
        return path.Replace('\\', '/');
    }

    private static bool PathMatchesScope(string rel, string scope)
    {
        if (string.IsNullOrWhiteSpace(scope))
        {
            return false;
        }

        if (rel.Contains(scope, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        string normalizedScope = scope.Replace('\\', '/').Trim();
        string mappedScope = normalizedScope switch
        {
            var value when value.StartsWith("/application/", StringComparison.OrdinalIgnoreCase) => ".Application/" + value["/application/".Length..],
            var value when value.StartsWith("/domain/", StringComparison.OrdinalIgnoreCase) => ".Domain/" + value["/domain/".Length..],
            var value when value.StartsWith("/infrastructure/", StringComparison.OrdinalIgnoreCase) => ".Infrastructure/" + value["/infrastructure/".Length..],
            var value when value.StartsWith("/controller/", StringComparison.OrdinalIgnoreCase) => ".Controller/" + value["/controller/".Length..],
            var value when value.StartsWith("/bootstrap/", StringComparison.OrdinalIgnoreCase) => ".Bootstrap/" + value["/bootstrap/".Length..],
            _ => normalizedScope.TrimStart('/')
        };

        return rel.Contains(mappedScope, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAllowedWorkflowDependency(ITypeSymbol parameterType, List<string> allowedDependencyTypeNames)
    {
        return allowedDependencyTypeNames.Any(allowed =>
            string.Equals(parameterType.Name, allowed, StringComparison.Ordinal) &&
            string.Equals(parameterType.ContainingNamespace?.ToDisplayString(), "MediatR", StringComparison.Ordinal));
    }

    private static bool TryGetInjectedDependencyTarget(
        SemanticModel semanticModel,
        INamedTypeSymbol? containingType,
        InvocationExpressionSyntax invocation,
        out ISymbol? receiverSymbol,
        out IMethodSymbol? targetMethodSymbol)
    {
        receiverSymbol = null;
        targetMethodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (targetMethodSymbol == null || invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return false;
        }

        receiverSymbol = semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
        if (receiverSymbol is not IFieldSymbol and not IPropertySymbol)
        {
            return false;
        }

        if (containingType == null)
        {
            return true;
        }

        return SymbolEqualityComparer.Default.Equals(receiverSymbol.ContainingType, containingType);
    }

    private static bool IsAllowedWorkflowDispatchCall(ISymbol? receiverSymbol, IMethodSymbol? targetMethodSymbol)
    {
        if (receiverSymbol is not IFieldSymbol and not IPropertySymbol)
        {
            return false;
        }

        ITypeSymbol? receiverType = receiverSymbol switch
        {
            IFieldSymbol field => field.Type,
            IPropertySymbol property => property.Type,
            _ => null
        };

        return receiverType != null &&
               string.Equals(receiverType.Name, "ISender", StringComparison.Ordinal) &&
               string.Equals(receiverType.ContainingNamespace?.ToDisplayString(), "MediatR", StringComparison.Ordinal) &&
               string.Equals(targetMethodSymbol?.Name, "Send", StringComparison.Ordinal);
    }

    private static bool IsForbiddenMediatorType(ITypeSymbol parameterType, List<string> forbiddenTypeNames)
    {
        return string.Equals(parameterType.ContainingNamespace?.ToDisplayString(), "MediatR", StringComparison.Ordinal) &&
               forbiddenTypeNames.Any(name => string.Equals(parameterType.Name, name, StringComparison.Ordinal));
    }

    private static bool IsForbiddenMediatorInvocation(IMethodSymbol methodSymbol, List<string> forbiddenInvocationNames)
    {
        return methodSymbol.ContainingType != null &&
               string.Equals(methodSymbol.ContainingType.ContainingNamespace?.ToDisplayString(), "MediatR", StringComparison.Ordinal) &&
               forbiddenInvocationNames.Any(name => string.Equals(methodSymbol.Name, name, StringComparison.Ordinal));
    }

    private static bool IsAllowedControllerDependency(ITypeSymbol parameterType, LinterPolicy policy)
    {
        if (policy.ControllerWorkflowRule.AllowedDependencyTypeNames.Any(name => string.Equals(parameterType.Name, name, StringComparison.Ordinal)))
        {
            return true;
        }

        if (policy.ControllerWorkflowRule.AllowedDependencySuffixes.Any(suffix => parameterType.Name.EndsWith(suffix, StringComparison.Ordinal)))
        {
            return true;
        }

        return parameterType switch
        {
            INamedTypeSymbol named when string.Equals(named.ContainingNamespace?.ToDisplayString(), "Microsoft.Extensions.Logging", StringComparison.Ordinal) &&
                                          string.Equals(named.Name, "ILogger", StringComparison.Ordinal) => true,
            INamedTypeSymbol named when string.Equals(named.ContainingNamespace?.ToDisplayString(), "Microsoft.Extensions.Configuration", StringComparison.Ordinal) &&
                                          string.Equals(named.Name, "IConfiguration", StringComparison.Ordinal) => true,
            _ => false
        };
    }

    private static bool IsAllowedConstructorDependency(ITypeSymbol parameterType)
    {
        if (parameterType.TypeKind == TypeKind.Interface)
        {
            return true;
        }

        if (parameterType.SpecialType != SpecialType.None)
        {
            return true;
        }

        if (parameterType.TypeKind is TypeKind.TypeParameter or TypeKind.Error or TypeKind.Dynamic)
        {
            return true;
        }

        if (parameterType is IArrayTypeSymbol or IPointerTypeSymbol)
        {
            return true;
        }

        if (parameterType is INamedTypeSymbol namedType && namedType.IsTupleType)
        {
            return true;
        }

        return IsAllowedNonInterfaceType(parameterType.Name) ||
               string.Equals(parameterType.ContainingNamespace?.ToDisplayString(), "System", StringComparison.Ordinal);
    }

    private static void AddForbiddenNamespaceHit(HashSet<string> forbiddenHits, ISymbol? symbol, List<string> forbiddenNamespaces)
    {
        foreach (string forbiddenNamespace in forbiddenNamespaces)
        {
            if (SymbolReferencesNamespace(symbol, forbiddenNamespace))
            {
                forbiddenHits.Add(forbiddenNamespace);
            }
        }
    }

    private static bool SymbolReferencesNamespace(ISymbol? symbol, string forbiddenNamespace)
    {
        if (symbol == null)
        {
            return false;
        }

        foreach (string namespaceValue in EnumerateCandidateNamespaces(symbol))
        {
            if (namespaceValue.StartsWith(forbiddenNamespace, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<string> EnumerateCandidateNamespaces(ISymbol symbol)
    {
        if (symbol is IAliasSymbol alias)
        {
            foreach (string candidate in EnumerateCandidateNamespaces(alias.Target))
            {
                yield return candidate;
            }

            yield break;
        }

        string containingNamespace = symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(containingNamespace))
        {
            yield return containingNamespace;
        }

        if (symbol is IMethodSymbol method)
        {
            foreach (string candidate in EnumerateTypeNamespaces(method.ReturnType))
            {
                yield return candidate;
            }

            foreach (IParameterSymbol parameter in method.Parameters)
            {
                foreach (string candidate in EnumerateTypeNamespaces(parameter.Type))
                {
                    yield return candidate;
                }
            }
        }
        else if (symbol is IPropertySymbol property)
        {
            foreach (string candidate in EnumerateTypeNamespaces(property.Type))
            {
                yield return candidate;
            }
        }
        else if (symbol is IFieldSymbol field)
        {
            foreach (string candidate in EnumerateTypeNamespaces(field.Type))
            {
                yield return candidate;
            }
        }
        else if (symbol is ILocalSymbol local)
        {
            foreach (string candidate in EnumerateTypeNamespaces(local.Type))
            {
                yield return candidate;
            }
        }
        else if (symbol is IParameterSymbol parameter)
        {
            foreach (string candidate in EnumerateTypeNamespaces(parameter.Type))
            {
                yield return candidate;
            }
        }
        else if (symbol is ITypeSymbol type)
        {
            foreach (string candidate in EnumerateTypeNamespaces(type))
            {
                yield return candidate;
            }
        }
    }

    private static IEnumerable<string> EnumerateTypeNamespaces(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol == null)
        {
            yield break;
        }

        string containingNamespace = typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(containingNamespace))
        {
            yield return containingNamespace;
        }

        if (typeSymbol is INamedTypeSymbol namedType)
        {
            foreach (ITypeSymbol typeArgument in namedType.TypeArguments)
            {
                foreach (string candidate in EnumerateTypeNamespaces(typeArgument))
                {
                    yield return candidate;
                }
            }
        }

        if (typeSymbol is IArrayTypeSymbol arrayType)
        {
            foreach (string candidate in EnumerateTypeNamespaces(arrayType.ElementType))
            {
                yield return candidate;
            }
        }
    }
}
