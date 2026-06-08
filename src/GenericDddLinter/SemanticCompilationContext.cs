using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

internal sealed class SemanticCompilationContext
{
    private readonly Dictionary<string, SyntaxTree> _syntaxTreesByPath;
    private readonly Dictionary<SyntaxTree, SemanticModel> _semanticModels;

    private SemanticCompilationContext(CSharpCompilation compilation, Dictionary<string, SyntaxTree> syntaxTreesByPath)
    {
        Compilation = compilation;
        _syntaxTreesByPath = syntaxTreesByPath;
        _semanticModels = new Dictionary<SyntaxTree, SemanticModel>();
    }

    public CSharpCompilation Compilation { get; }

    public static SemanticCompilationContext Create(string repoRoot, List<string> files)
    {
        Dictionary<string, SyntaxTree> syntaxTreesByPath = files.ToDictionary(
            path => path,
            path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), path: path),
            StringComparer.OrdinalIgnoreCase);

        List<MetadataReference> references = CollectMetadataReferences(repoRoot);
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "GenericDddLinter.SemanticCompilation",
            syntaxTrees: syntaxTreesByPath.Values,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return new SemanticCompilationContext(compilation, syntaxTreesByPath);
    }

    public SyntaxTree GetSyntaxTree(string filePath)
    {
        return _syntaxTreesByPath[filePath];
    }

    public SemanticModel GetSemanticModel(string filePath)
    {
        SyntaxTree tree = GetSyntaxTree(filePath);
        if (_semanticModels.TryGetValue(tree, out SemanticModel? model))
        {
            return model;
        }

        SemanticModel created = Compilation.GetSemanticModel(tree, ignoreAccessibility: true);
        _semanticModels[tree] = created;
        return created;
    }

    public bool HasErrors(string filePath)
    {
        return GetSyntaxTree(filePath)
            .GetDiagnostics()
            .Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
    }

    private static List<MetadataReference> CollectMetadataReferences(string repoRoot)
    {
        HashSet<string> referencePaths = new(StringComparer.OrdinalIgnoreCase);

        string? trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (!string.IsNullOrWhiteSpace(trustedPlatformAssemblies))
        {
            foreach (string path in trustedPlatformAssemblies.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                referencePaths.Add(path);
            }
        }

        foreach (string assetFile in Directory.GetFiles(repoRoot, "project.assets.json", SearchOption.AllDirectories))
        {
            foreach (string referencePath in CollectPackageCompileReferences(assetFile))
            {
                referencePaths.Add(referencePath);
            }
        }

        return referencePaths
            .Where(File.Exists)
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
            .ToList();
    }

    private static IEnumerable<string> CollectPackageCompileReferences(string assetFilePath)
    {
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(assetFilePath));
        JsonElement root = document.RootElement;

        if (!root.TryGetProperty("packageFolders", out JsonElement packageFolders))
        {
            yield break;
        }

        string? packageRoot = packageFolders.EnumerateObject().Select(p => p.Name).FirstOrDefault();
        if (string.IsNullOrWhiteSpace(packageRoot))
        {
            yield break;
        }

        if (!root.TryGetProperty("targets", out JsonElement targets))
        {
            yield break;
        }

        foreach (JsonProperty target in targets.EnumerateObject())
        {
            foreach (JsonProperty library in target.Value.EnumerateObject())
            {
                if (!library.Value.TryGetProperty("compile", out JsonElement compile))
                {
                    continue;
                }

                string packageName = library.Name.Replace('/', Path.DirectorySeparatorChar);
                foreach (JsonProperty compileEntry in compile.EnumerateObject())
                {
                    string fullPath = Path.Combine(packageRoot, packageName, compileEntry.Name.Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(fullPath))
                    {
                        yield return fullPath;
                    }
                }
            }
        }
    }
}
