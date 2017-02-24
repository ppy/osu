// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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