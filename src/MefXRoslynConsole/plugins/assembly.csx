#r "plugins/MefXRoslynRandomAssembly.dll"

[Export(typeof(IPlugin))]
public class PluginWithAssembly : IPlugin
{
    public string GetInfo() => nameof(MefXRoslynRandomAssembly.Class1);
}
