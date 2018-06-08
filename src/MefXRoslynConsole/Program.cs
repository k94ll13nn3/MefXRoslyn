using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MefXRoslynLibrary;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace MefXRoslynConsole
{
    internal class Program
    {
        private readonly string pluginPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "plugins");

        private List<MetadataReference> platformAssemblies;

        private Program()
        {
            platformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")
                .ToString()
                .Split(Path.PathSeparator)
                .Where(t => t.Contains("System.Runtime.dll") || t.Contains("System.Private.CoreLib.dll") || t.Contains("System.Console.dll"))
                .Select(x => (MetadataReference)MetadataReference.CreateFromFile(x))
                .ToList();

            platformAssemblies.Add(MetadataReference.CreateFromFile(typeof(IPlugin).Assembly.Location));
            platformAssemblies.Add(MetadataReference.CreateFromFile(typeof(ExportAttribute).Assembly.Location));

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            catalog.Catalogs.Add(new DirectoryCatalog(pluginPath, "*.dll"));
            foreach (Assembly plugin in LoadPluginsFromScripts())
            {
                catalog.Catalogs.Add(new AssemblyCatalog(plugin));
            }

            Container = new CompositionContainer(catalog);

            try
            {
                Container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                Console.WriteLine(compositionException.ToString());
            }
        }

        public CompositionContainer Container { get; set; }

        private static void Main(string[] args)
        {
            new Program().Container.GetExportedValue<PluginTester>().DoWork();
        }

        private IEnumerable<Assembly> LoadPluginsFromScripts()
        {
            var assemblies = new List<Assembly>();
            foreach (var script in Directory.EnumerateFiles(pluginPath, "*.csx"))
            {
                Assembly plugin = LoadPluginFromScript(script);
                if (plugin != null)
                {
                    assemblies.Add(plugin);
                }
            }

            return assemblies;
        }

        private Assembly LoadPluginFromScript(string path)
        {
            Console.WriteLine($"Loading file {path}...");

            var text = new StringBuilder();
            text.Append("using System.ComponentModel.Composition;");
            text.Append("using MefXRoslynLibrary;");
            text.Append(File.ReadAllText(path));

            var parseOptions = new CSharpParseOptions(
                languageVersion: LanguageVersion.Latest,
                kind: SourceCodeKind.Regular);

            var compilationOptions = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release);

            SyntaxTree tree = CSharpSyntaxTree.ParseText(text.ToString(), parseOptions);
            var compilation = CSharpCompilation.Create(
                Path.GetFileNameWithoutExtension(path),
                new[] { tree },
                platformAssemblies,
                compilationOptions);

            using (var memoryStream = new MemoryStream())
            {
                EmitResult emitResult = compilation.Emit(memoryStream);
                foreach (Diagnostic diagnostic in emitResult.Diagnostics)
                {
                    PrintDiagnostic(diagnostic);
                }

                if (!emitResult.Success)
                {
                    return null;
                }
                else
                {
                    // Cannot load into another AppDomain in .NET Core
                    var assembly = Assembly.Load(memoryStream.ToArray());
                    foreach (Type type in assembly.GetTypes())
                    {
                        Console.WriteLine($"Loaded type {type.FullName}.");
                    }

                    Console.Write(Environment.NewLine);

                    return assembly;
                }
            }
        }

        private void PrintDiagnostic(Diagnostic diagnostic)
        {
            ConsoleColor defaultForegroundColor = Console.ForegroundColor;
            ConsoleColor newForegroundColor = defaultForegroundColor;
            switch (diagnostic.Severity)
            {
                case DiagnosticSeverity.Info:
                    newForegroundColor = ConsoleColor.Cyan;
                    break;

                case DiagnosticSeverity.Warning:
                    newForegroundColor = ConsoleColor.Yellow;
                    break;

                case DiagnosticSeverity.Error:
                    newForegroundColor = ConsoleColor.Red;
                    break;
            }

            Console.ForegroundColor = newForegroundColor;
            Console.WriteLine(diagnostic);
            Console.ForegroundColor = defaultForegroundColor;
        }
    }
}