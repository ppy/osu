using System;
using System.ComponentModel;

namespace osu.Game.Configuration
{
    public enum ScreenshotFormat
    {
        Bmp = 0, // TODO: Figure out the best way to hide this from the dropdown
        [Description("JPG (web-friendly)")]
        Jpg = 1,
        [Description("PNG (lossless)")]
        Png = 2
    }
}