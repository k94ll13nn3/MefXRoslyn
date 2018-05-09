using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MefXRoslynLibrary;

namespace MefXRoslynConsole
{
    [Export(typeof(PluginTester))]
    public class PluginTester
    {
        private readonly IEnumerable<IPlugin> plugins;
        private readonly IEnumerable<Lazy<IFunction, IFunctionData>> functions;

        [ImportingConstructor]
        public PluginTester(
            [ImportMany] IEnumerable<IPlugin> plugins,
            [ImportMany] IEnumerable<Lazy<IFunction, IFunctionData>> functions)
        {
            this.plugins = plugins;
            this.functions = functions;
        }

        public void DoWork()
        {
            foreach (IPlugin plugin in plugins)
            {
                Console.WriteLine(plugin.GetInfo());
            }

            var random = new Random();
            foreach (Lazy<IFunction, IFunctionData> function in functions)
            {
                var left = random.Next(0, 100);
                var right = random.Next(0, 100);
                Console.WriteLine($"{left} {function.Metadata.Symbol} {right} = {function.Value.Operate(left, right)}.");
            }
        }
    }
}