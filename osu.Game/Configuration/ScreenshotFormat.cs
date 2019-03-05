// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
