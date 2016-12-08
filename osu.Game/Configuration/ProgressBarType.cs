using System;
using System.ComponentModel;

namespace osu.Game.Configuration
{
    public enum ProgressBarType
    {
        Off,
        Pie,
        [Description("Top Right")]
        TopRight,
        [Description("Bottom Right")]
        BottomRight,
        Bottom
    }
}