using System.ComponentModel;

namespace osu.Game.Modes.Osu
{
    public enum ComboJudgement
    {
        [Description(@"")]
        None,
        [Description(@"Good")]
        Good,
        [Description(@"Amazing")]
        Perfect
    }
}