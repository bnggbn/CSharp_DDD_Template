using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class RoslynRuleRunner
{
    public static void Run(LinterPolicy policy, List<string> files, List<string> issues)
    {
        CheckUseCaseFileRules(policy, files, issues);
        CheckCqrsCommandFileRules(policy, files, issues);
        CheckCqrsQueryFileRules(policy, files, issues);
        CheckSeverityMutationRule(policy, files, issues);
        CheckConstructorInterfaceRule(policy, files, issues);
        CheckConstantsClassRule(policy, files, issues);
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
            if (!rel.Contains(policy.UseCaseFileRule.UseCasesPathContains, StringComparison.OrdinalIgnoreCase))
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
            if (!rel.Contains(pathContains, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string text = File.ReadAllText(file);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(text);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

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

    private static void CheckConstructorInterfaceRule(LinterPolicy policy, List<string> files, List<string> issues)
    {
        if (!policy.ConstructorInterfaceRule.Enabled)
        {
            return;
        }

        foreach (string file in files)
        {
            string rel = Normalize(file);
            if (!policy.ConstructorInterfaceRule.TargetPathContains.Any(path => rel.Contains(path, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            string text = File.ReadAllText(file);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(text);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            foreach (ConstructorDeclarationSyntax ctor in root.DescendantNodes().OfType<ConstructorDeclarationSyntax>())
            {
                foreach (ParameterSyntax parameter in ctor.ParameterList.Parameters)
                {
                    string typeName = GetTerminalTypeName(parameter.Type);
                    if (string.IsNullOrWhiteSpace(typeName) || IsAllowedNonInterfaceType(typeName))
                    {
                        continue;
                    }

                    if (!typeName.StartsWith("I", StringComparison.Ordinal))
                    {
                        issues.Add($"[{policy.ConstructorInterfaceRule.RuleId}] {rel}: constructor dependency '{typeName}' should depend on interface.");
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
            if (!policy.ConstantsClassRule.TargetPathContains.Any(path => rel.Contains(path, StringComparison.OrdinalIgnoreCase)))
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
}
