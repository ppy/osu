using System;
using IToggleableFunctionProvider = osu.Game.Screens.LLin.Plugins.Types.IToggleableFunctionProvider;

namespace osu.Game.Screens.Mvis.Plugins.Internal.FallbackFunctionBar
{
    [Obsolete("原Mvis播放器现已移动至LLin(osu.Game.Screens.LLin)")]
    public class ToggleableBarButton : osu.Game.Screens.LLin.Plugins.Internal.FallbackFunctionBar.ToggleableBarButton
    {
        public ToggleableBarButton(IToggleableFunctionProvider provider)
            : base(provider)
        {
        }
    }
}
