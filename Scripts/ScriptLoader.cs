using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using GraphicsPlayground.Graphics.Render;
using GraphicsPlayground.Util;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;

namespace GraphicsPlayground.Scripts;

/// <summary>Loads scripts from the given directory.</summary>
public static class ScriptLoader
{
    /// <summary>Directory to load scripts from.</summary>
    public static string? ScriptPath;

    /// <summary>
    /// Loads all scripts in the build path you can specify a custom assembly to load from for external dependencies.
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="assembly"></param>
    /// <exception cref="Exception"></exception>
    public static void LoadAllScripts(Engine engine, Assembly? assembly = null)
    {
        if (ScriptPath == null)
        {
            throw new Exception("Script path not set for script loader.");
        }
        Assembly? coreAssembly = Assembly.LoadFrom("space.dll") ?? throw new Exception("Failed to load space assembly.");
        AssemblyName[] assemblyReferences = assembly?.GetReferencedAssemblies() ?? Array.Empty<AssemblyName>();
        AssemblyName[] coreReferences = coreAssembly.GetReferencedAssemblies();
        foreach (AssemblyName assemblyRef in assemblyReferences)
        {
            if (!coreReferences.Contains(assemblyRef))
            {
                coreReferences.Append(assemblyRef);
            }
        }

        // TODO: Load scripts async.
        //static void afterScriptLoaded()

        string[] files = Directory.GetFiles(ScriptPath, "*.cs", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            string sourceCode = File.ReadAllText(file);
            string assemblyName = Path.GetFileNameWithoutExtension(file);
            Assembly scriptAssembly = LoadScript(sourceCode, assemblyName, coreReferences, coreAssembly);
            Type[] types = scriptAssembly.GetTypes();
            foreach (Type type in types)
            {
                if (type.GetInterfaces().Contains(typeof(IScript)))
                {
                    if (Activator.CreateInstance(type) is not IScript script)
                    {
                        DebugLogger.Log($"<red>Failed to load script: <white>{type.Name}");
                        continue;
                    }
                    if (!script.IsEnabled ?? true) return;
                    script.OnLoad(engine);
                    engine.Scripts.Add(script);
                    DebugLogger.Log($"<aqua>Loaded script: <white>{type.Name}");
                }
            }
        }
    }

    public static Assembly LoadScript(string sourceCode, string assemblyName, AssemblyName[] assemblies, Assembly coreAssembly)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();

        UsingDirectiveSyntax[] defaultUsings = new UsingDirectiveSyntax[]
        {
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Linq")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Threading.Tasks"))
        };

        root = root.AddUsings(defaultUsings);
        SyntaxTree newTree = CSharpSyntaxTree.Create(root);

        CSharpCompilationOptions options = new(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true);
        CSharpCompilation compilation = CSharpCompilation.Create(assemblyName)
            .WithOptions(options)
            .AddReferences(assemblies.Select(Assembly.Load).Select(x => MetadataReference.CreateFromFile(x.Location)))
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(coreAssembly.Location))
            .AddSyntaxTrees(newTree);

        using MemoryStream stream = new();
        EmitResult result = compilation.Emit(stream);
        if (!result.Success)
        {
            IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                           diagnostic.IsWarningAsError ||
                                          diagnostic.Severity == DiagnosticSeverity.Error);
            StringBuilder builder = new();
            foreach (Diagnostic diagnostic in failures)
            {
                builder.AppendLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
            }
            builder.AppendLine($"Failed to load script {assemblyName}. ");
            throw new Exception(builder.ToString());
        }
        stream.Seek(0, SeekOrigin.Begin);

        return Assembly.Load(stream.ToArray());
    }
}
