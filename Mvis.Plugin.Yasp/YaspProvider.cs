using osu.Game.Screens.Mvis.Plugins;

namespace Mvis.Plugin.Yasp
{
    public class YaspProvider : MvisPluginProvider
    {
        //在这里制定该Provider要提供的插件
        public override MvisPlugin CreatePlugin => new YaspPlugin();
    }
}
