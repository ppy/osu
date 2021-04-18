// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Utils
{
    public static class ColourUtils
    {
        /// <summary>
        /// Returns a foreground text colour that is supposed to contrast well on top of
        /// the supplied <paramref name="backgroundColour"/>.
        /// </summary>
        public static Color4 ForegroundTextColourFor(Color4 backgroundColour)
        {
            // formula taken from the RGB->YIQ conversions: https://en.wikipedia.org/wiki/YIQ
            // brightness here is equivalent to the Y component in the above colour model, which is a rough estimate of lightness.
            float brightness = 0.299f * backgroundColour.R + 0.587f * backgroundColour.G + 0.114f * backgroundColour.B;
            return OsuColour.Gray(brightness > 0.5f ? 0.2f : 0.9f);
        }
    }
}
