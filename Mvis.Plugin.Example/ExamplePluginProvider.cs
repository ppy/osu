using osu.Game.Screens.Mvis.Plugins;

namespace Mvis.Plugin.Example
{
    public class ExamplePluginProvider : MvisPluginProvider
    {
        public override MvisPlugin CreatePlugin => new ExamplePlugin();
    }
}
