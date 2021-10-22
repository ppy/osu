using System;
using osuTK.Input;

namespace osu.Game.Screens.Mvis.Plugins
{
    [Obsolete("原Mvis播放器现已移动至LLin(osu.Game.Screens.LLin)")]
    public class PluginKeybind : osu.Game.Screens.LLin.Plugins.PluginKeybind
    {
        public PluginKeybind(Key key, Action action, string name = "???")
            : base(key, action, name)
        {
        }
    }
}
