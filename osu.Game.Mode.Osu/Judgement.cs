using System.ComponentModel;

namespace osu.Game.Modes.Osu
{
    public enum Judgement
    {
        [Description(@"Miss")]
        Miss,
        [Description(@"50")]
        Hit50,
        [Description(@"100")]
        Hit100,
        [Description(@"300")]
        Hit300,
        [Description(@"500")]
        Hit500
    }
}