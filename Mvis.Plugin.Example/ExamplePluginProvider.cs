using osu.Game.Screens.Mvis.Plugins;

namespace Mvis.Plugin.Example
{
    public class ExamplePluginProvider : MvisPluginProvider
    {
        //在这里制定该Provider要提供的插件
        public override MvisPlugin CreatePlugin => new ExamplePlugin();
    }
}
