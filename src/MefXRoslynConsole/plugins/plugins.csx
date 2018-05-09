[Export(typeof(IPlugin))]
public class PluginInScript : IPlugin
{
    public string GetInfo() => nameof(PluginInScript);
}

public class NotExportedPlugin : IPlugin
{
    public string GetInfo() => nameof(NotExportedPlugin);
}