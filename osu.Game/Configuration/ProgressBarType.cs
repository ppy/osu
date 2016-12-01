using System;

namespace osu.Game.Configuration
{
    public enum ProgressBarType
    {
        Off,
        Pie,
        [DisplayName("Top Right")]
        TopRight,
        [DisplayName("Bottom Right")]
        BottomRight,
        Bottom
    }
}