using System;
using IFunctionProvider = osu.Game.Screens.LLin.Plugins.Types.IFunctionProvider;

namespace osu.Game.Screens.Mvis.Plugins.Internal.FallbackFunctionBar
{
    [Obsolete("原Mvis播放器现已移动至LLin(osu.Game.Screens.LLin)")]
    public class SimpleBarButton : osu.Game.Screens.LLin.Plugins.Internal.FallbackFunctionBar.SimpleBarButton
    {
        public SimpleBarButton(IFunctionProvider provider)
            : base(provider)
        {
        }
    }
}
