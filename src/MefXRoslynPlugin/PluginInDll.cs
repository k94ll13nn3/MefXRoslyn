using System.ComponentModel.Composition;
using MefXRoslynLibrary;

namespace MefXRoslynPlugin
{
    [Export(typeof(IPlugin))]
    public class PluginInDll : IPlugin
    {
        public string GetInfo() => nameof(PluginInDll);
    }
}