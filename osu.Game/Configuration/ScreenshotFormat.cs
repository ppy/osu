using System;
namespace osu.Game.Configuration
{
    public enum ScreenshotFormat
    {
        Bmp = 0, // TODO: Figure out the best way to hide this from the dropdown
        [DisplayName("JPG (web-friendly)")]
        Jpg = 1,
        [DisplayName("PNG (lossless)")]
        Png = 2
    }
}