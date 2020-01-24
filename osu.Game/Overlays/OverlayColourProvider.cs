// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public class OverlayColourProvider
    {
        private readonly OverlayColourScheme colourScheme;

        public OverlayColourProvider(OverlayColourScheme colourScheme)
        {
            this.colourScheme = colourScheme;
        }

        private Color4 convert(float saturation, float lightness) => Color4.FromHsl(new Vector4(getBaseHue(colourScheme), saturation, lightness, 1));

        // See https://github.com/ppy/osu-web/blob/4218c288292d7c810b619075471eaea8bbb8f9d8/app/helpers.php#L1463
        private static float getBaseHue(OverlayColourScheme colourScheme)
        {
            float hue;

            switch (colourScheme)
            {
                default:
                    throw new ArgumentException($@"{colourScheme} colour scheme does not provide a hue value in {nameof(getBaseHue)}.");

                case OverlayColourScheme.Red:
                    hue = 0;
                    break;

                case OverlayColourScheme.Pink:
                    hue = 333;
                    break;

                case OverlayColourScheme.Orange:
                    hue = 46;
                    break;

                case OverlayColourScheme.Green:
                    hue = 115;
                    break;

                case OverlayColourScheme.Purple:
                    hue = 255;
                    break;

                case OverlayColourScheme.Blue:
                    hue = 200;
                    break;
            }

            return hue / 360f;
        }

        public Color4 Highlight1 => convert(1, 0.7f);
        public Color4 Content1 => convert(0.4f, 1);
        public Color4 Content2 => convert(0.4f, 0.9f);
        public Color4 Link1 => convert(0.4f, 0.8f);
        public Color4 Link2 => convert(0.4f, 0.75f);
        public Color4 Link3 => convert(0.4f, 0.7f);
        public Color4 Link4 => convert(0.4f, 0.5f);
        public Color4 Dark1 => convert(0.2f, 0.35f);
        public Color4 Dark2 => convert(0.2f, 0.3f);
        public Color4 Dark3 => convert(0.2f, 0.25f);
        public Color4 Dark4 => convert(0.2f, 0.2f);
        public Color4 Dark5 => convert(0.2f, 0.15f);
        public Color4 Dark6 => convert(0.2f, 0.1f);
        public Color4 Foreground1 => convert(0.1f, 0.6f);
        public Color4 Background1 => convert(0.1f, 0.4f);
        public Color4 Background2 => convert(0.1f, 0.3f);
        public Color4 Background3 => convert(0.1f, 0.25f);
        public Color4 Background4 => convert(0.1f, 0.2f);
        public Color4 Background5 => convert(0.1f, 0.15f);
        public Color4 Background6 => convert(0.1f, 0.1f);
    }

    public enum OverlayColourScheme
    {
        Red,
        Pink,
        Orange,
        Green,
        Purple,
        Blue
    }
}
