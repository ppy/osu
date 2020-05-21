// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
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

        public Colour4 Highlight1 => getColour(1, 0.7f);
        public Colour4 Content1 => getColour(0.4f, 1);
        public Colour4 Content2 => getColour(0.4f, 0.9f);
        public Colour4 Light1 => getColour(0.4f, 0.8f);
        public Colour4 Light2 => getColour(0.4f, 0.75f);
        public Colour4 Light3 => getColour(0.4f, 0.7f);
        public Colour4 Light4 => getColour(0.4f, 0.5f);
        public Colour4 Dark1 => getColour(0.2f, 0.35f);
        public Colour4 Dark2 => getColour(0.2f, 0.3f);
        public Colour4 Dark3 => getColour(0.2f, 0.25f);
        public Colour4 Dark4 => getColour(0.2f, 0.2f);
        public Colour4 Dark5 => getColour(0.2f, 0.15f);
        public Colour4 Dark6 => getColour(0.2f, 0.1f);
        public Colour4 Foreground1 => getColour(0.1f, 0.6f);
        public Colour4 Background1 => getColour(0.1f, 0.4f);
        public Colour4 Background2 => getColour(0.1f, 0.3f);
        public Colour4 Background3 => getColour(0.1f, 0.25f);
        public Colour4 Background4 => getColour(0.1f, 0.2f);
        public Colour4 Background5 => getColour(0.1f, 0.15f);
        public Colour4 Background6 => getColour(0.1f, 0.1f);

        private Colour4 getColour(float saturation, float lightness) => Colour4.FromHsl(new Vector4(getBaseHue(colourScheme), saturation, lightness, 1));

        // See https://github.com/ppy/osu-web/blob/4218c288292d7c810b619075471eaea8bbb8f9d8/app/helpers.php#L1463
        private static float getBaseHue(OverlayColourScheme colourScheme)
        {
            switch (colourScheme)
            {
                default:
                    throw new ArgumentException($@"{colourScheme} colour scheme does not provide a hue value in {nameof(getBaseHue)}.");

                case OverlayColourScheme.Red:
                    return 0;

                case OverlayColourScheme.Pink:
                    return 333 / 360f;

                case OverlayColourScheme.Orange:
                    return 46 / 360f;

                case OverlayColourScheme.Green:
                    return 115 / 360f;

                case OverlayColourScheme.Purple:
                    return 255 / 360f;

                case OverlayColourScheme.Blue:
                    return 200 / 360f;
            }
        }
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
