using System;

namespace osu.Game.Screens.Mvis.Plugins.Types
{
    [Obsolete("原Mvis播放器现已移动至LLin(osu.Game.Screens.LLin)")]
    public interface IFunctionProvider : osu.Game.Screens.LLin.Plugins.Types.IFunctionProvider
    {
    }

    [Obsolete("原Mvis播放器现已移动至LLin(osu.Game.Screens.LLin)")]
    public interface IToggleableFunctionProvider : osu.Game.Screens.LLin.Plugins.Types.IToggleableFunctionProvider
    {
    }

    [Obsolete("原Mvis播放器现已移动至LLin(osu.Game.Screens.LLin)")]
    public interface IPluginFunctionProvider : osu.Game.Screens.LLin.Plugins.Types.IPluginFunctionProvider
    {
    }

    [Obsolete("原Mvis播放器现已移动至LLin(osu.Game.Screens.LLin)")]
    public enum FunctionType
    {
        Base,
        Audio,
        Plugin,
        Misc,
        ProgressDisplay
    }
}
