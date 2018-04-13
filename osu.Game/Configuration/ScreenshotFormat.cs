// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;

namespace osu.Game.Configuration
{
    public enum ScreenshotFormat
    {
        [Description("JPG (web-friendly)")]
        Jpg = 1,
        [Description("PNG (lossless)")]
        Png = 2
    }
}
