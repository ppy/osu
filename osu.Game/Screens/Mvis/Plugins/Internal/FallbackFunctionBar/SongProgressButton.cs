using System;
using IToggleableFunctionProvider = osu.Game.Screens.LLin.Plugins.Types.IToggleableFunctionProvider;

namespace osu.Game.Screens.Mvis.Plugins.Internal.FallbackFunctionBar
{
    [Obsolete("原Mvis播放器现已移动至LLin(osu.Game.Screens.LLin)")]
    public class SongProgressButton : osu.Game.Screens.LLin.Plugins.Internal.FallbackFunctionBar.SongProgressButton
    {
        public SongProgressButton(IToggleableFunctionProvider provider)
            : base(provider)
        {
        }
    }
}
