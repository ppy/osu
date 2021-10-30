using osu.Game.Screens.LLin.Plugins;

namespace Mvis.Plugin.CloudMusicSupport
{
    public class LyricPluginProvider : LLinPluginProvider
    {
        //在这里制定该Provider要提供的插件
        public override LLinPlugin CreatePlugin => new LyricPlugin();
    }
}
