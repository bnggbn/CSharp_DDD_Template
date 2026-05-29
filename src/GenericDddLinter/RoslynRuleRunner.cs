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
        CheckCqrsCommandFileRules(policy, files, issues);
        CheckCqrsQueryFileRules(policy, files, issues);
        CheckSeverityMutationRule(policy, files, issues);
        CheckConstructorInterfaceRule(policy, files, issues);
        CheckConstantsClassRule(policy, files, issues);
    }

    private static void CheckCqrsCommandFileRules(LinterPolicy policy, List<string> files, List<string> issues)
    {
        ValidateRequestFileRule(files, issues, policy.CqrsCommandFileRule.RuleId, policy.CqrsCommandFileRule.CommandsPathContains, "Command");
    }

    private static void CheckCqrsQueryFileRules(LinterPolicy policy, List<string> files, List<string> issues)
    {
        ValidateRequestFileRule(files, issues, policy.CqrsQueryFileRule.RuleId, policy.CqrsQueryFileRule.QueriesPathContains, "Query");
    }

    private static void ValidateRequestFileRule(List<string> files, List<string> issues, string ruleId, string pathContains, string requestSuffix)
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
            List<string> requestTypes = types
                .Where(x => x.Identifier.Text.EndsWith(requestSuffix, StringComparison.Ordinal))
                .Select(x => x.Identifier.Text)
                .ToList();

            List<(string HandlerName, string RequestName)> handlers = new();
            foreach (TypeDeclarationSyntax type in types.Where(x => x.Identifier.Text.EndsWith("Handler", StringComparison.Ordinal)))
            {
                foreach (BaseTypeSyntax baseType in type.BaseList?.Types ?? Enumerable.Empty<BaseTypeSyntax>())
                {
                    if (baseType.Type is not GenericNameSyntax generic || generic.Identifier.Text != "IRequestHandler")
                    {
                        continue;
                    }

                    if (generic.TypeArgumentList.Arguments.Count < 1)
                    {
                        continue;
                    }

                    string requestName = generic.TypeArgumentList.Arguments[0].ToString().Split('.').Last();
                    handlers.Add((type.Identifier.Text, requestName));
                }
            }

            if (requestTypes.Count != 1)
            {
                issues.Add($"[{ruleId}] {rel}: file must contain exactly one *{requestSuffix} type.");
                continue;
            }

            if (handlers.Count != 1)
            {
                issues.Add($"[{ruleId}] {rel}: file must contain exactly one IRequestHandler for the {requestSuffix.ToLowerInvariant()}.");
                continue;
            }

            string requestType = requestTypes[0];
            if (!string.Equals(handlers[0].RequestName, requestType, StringComparison.Ordinal))
            {
                issues.Add($"[{ruleId}] {rel}: handler '{handlers[0].HandlerName}' must handle '{requestType}'.");
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
