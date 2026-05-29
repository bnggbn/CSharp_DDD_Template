using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

internal static class Program
{
    public static int Main(string[] args)
    {
        string repoRoot = args.Length > 0
            ? Path.GetFullPath(args[0])
            : Directory.GetCurrentDirectory();

        string policyPath = args.Length > 1
            ? Path.GetFullPath(args[1])
            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "linter.policy.sample.json");

        if (!File.Exists(policyPath))
        {
            Console.Error.WriteLine("Policy file not found: " + policyPath);
            return 2;
        }

        LinterPolicy policy = JsonSerializer.Deserialize<LinterPolicy>(File.ReadAllText(policyPath), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new LinterPolicy();

        List<string> csFiles = Directory.GetFiles(repoRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .ToList();

        List<string> issues = new();
        RegexRuleRunner.Run(policy, csFiles, issues);
        RoslynRuleRunner.Run(policy, csFiles, issues);

        if (issues.Count == 0)
        {
            Console.WriteLine("Lint passed: no issues found.");
            return 0;
        }

        foreach (string issue in issues)
        {
            Console.WriteLine(issue);
        }

        Console.WriteLine();
        Console.WriteLine("Lint failed: " + issues.Count + " issue(s).");
        return 1;
    }
}