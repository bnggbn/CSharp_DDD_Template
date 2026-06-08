using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
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
        RunBuildGate(policy, repoRoot, issues);
        RegexRuleRunner.Run(policy, csFiles, issues);
        RoslynRuleRunner.Run(policy, repoRoot, csFiles, issues);

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

    private static void RunBuildGate(LinterPolicy policy, string repoRoot, List<string> issues)
    {
        if (!policy.BuildGateRule.Enabled)
        {
            return;
        }

        if (policy.BuildGateRule.ExcludedProjectFileNames.Count > 0)
        {
            RunProjectBuildGate(policy, repoRoot, issues);
            return;
        }

        string? solutionPath = FindBuildTarget(repoRoot, policy.BuildGateRule.SolutionSearchPatterns);
        if (string.IsNullOrWhiteSpace(solutionPath))
        {
            issues.Add($"[{policy.BuildGateRule.RuleId}] /: lint requires a compilable solution, but no .slnx/.sln file was found in the repository root.");
            return;
        }

        ProcessStartInfo startInfo = new()
        {
            FileName = "dotnet",
            Arguments = $"build \"{solutionPath}\" --nologo --no-restore -p:UseAppHost=false -p:CreateAppHost=false",
            WorkingDirectory = repoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process? process = Process.Start(startInfo);
        if (process == null)
        {
            issues.Add($"[{policy.BuildGateRule.RuleId}] /: lint could not start 'dotnet build' for '{Path.GetFileName(solutionPath)}'.");
            return;
        }

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode == 0)
        {
            return;
        }

        string firstLine = (output + Environment.NewLine + error)
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? "dotnet build failed.";
        issues.Add($"[{policy.BuildGateRule.RuleId}] /: lint requires compilable code. Fix build errors first. Build target '{Path.GetFileName(solutionPath)}' failed: {firstLine}");
    }

    private static void RunProjectBuildGate(LinterPolicy policy, string repoRoot, List<string> issues)
    {
        HashSet<string> excludedProjectFileNames = new(policy.BuildGateRule.ExcludedProjectFileNames, StringComparer.OrdinalIgnoreCase);
        List<string> projectPaths = Directory.GetFiles(repoRoot, "*.csproj", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !excludedProjectFileNames.Contains(Path.GetFileName(path)))
            .ToList();

        if (projectPaths.Count == 0)
        {
            issues.Add($"[{policy.BuildGateRule.RuleId}] /: lint requires compilable projects, but no eligible .csproj files were found after exclusions.");
            return;
        }

        foreach (string projectPath in projectPaths)
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = "dotnet",
                Arguments = $"build \"{projectPath}\" --nologo -p:UseAppHost=false -p:CreateAppHost=false",
                WorkingDirectory = repoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process? process = Process.Start(startInfo);
            if (process == null)
            {
                issues.Add($"[{policy.BuildGateRule.RuleId}] /: lint could not start 'dotnet build' for '{Path.GetFileName(projectPath)}'.");
                return;
            }

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                continue;
            }

            string firstLine = (output + Environment.NewLine + error)
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault() ?? "dotnet build failed.";
            issues.Add($"[{policy.BuildGateRule.RuleId}] /: lint requires compilable code. Fix build errors first. Project '{Path.GetFileName(projectPath)}' failed: {firstLine}");
            return;
        }
    }

    private static string? FindBuildTarget(string repoRoot, List<string> searchPatterns)
    {
        foreach (string pattern in searchPatterns)
        {
            string[] matches = Directory.GetFiles(repoRoot, pattern, SearchOption.TopDirectoryOnly);
            if (matches.Length == 1)
            {
                return matches[0];
            }
        }

        foreach (string pattern in searchPatterns)
        {
            string[] matches = Directory.GetFiles(repoRoot, pattern, SearchOption.TopDirectoryOnly);
            if (matches.Length > 1)
            {
                return null;
            }
        }

        return null;
    }
}
