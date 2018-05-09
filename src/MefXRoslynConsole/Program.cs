using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
                .Split(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ';' : ':')
                .Select(x => (MetadataReference)MetadataReference.CreateFromFile(x))
                .ToList();

            MetadataReference mefScriptConsole = MetadataReference.CreateFromFile(typeof(IPlugin).Assembly.Location);
            MetadataReference systemComponentModelComposition = MetadataReference.CreateFromFile(typeof(ExportAttribute).Assembly.Location);

            platformAssemblies.Add(mefScriptConsole);
            platformAssemblies.Add(systemComponentModelComposition);

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

            SyntaxTree tree = CSharpSyntaxTree.ParseText(text.ToString());

            var options = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release);

            var compilation = CSharpCompilation.Create(
                Path.GetFileNameWithoutExtension(path),
                new[] { tree },
                platformAssemblies,
                options);

            var memoryStream = new MemoryStream();
            EmitResult emitResult = compilation.Emit(memoryStream);
            if (!emitResult.Success)
            {
                foreach (Diagnostic item in emitResult.Diagnostics)
                {
                    Console.WriteLine(item);
                }

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
}