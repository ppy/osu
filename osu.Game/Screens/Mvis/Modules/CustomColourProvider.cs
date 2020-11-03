// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Mvis.Modules
{
    ///<summary>
    ///更改自<see cref="OverlayColourProvider"/>
    ///</summary>
    public class CustomColourProvider
    {
        public Color4 Highlight1 => getColour(1, 0.7f);
        public Color4 Content1 => getColour(0.4f, 1);
        public Color4 Content2 => getColour(0.4f, 0.9f);
        public Color4 Light1 => getColour(0.4f, 0.8f);
        public Color4 Light2 => getColour(0.4f, 0.75f);
        public Color4 Light3 => getColour(0.4f, 0.7f);
        public Color4 Light4 => getColour(0.4f, 0.5f);
        public Color4 Dark1 => getColour(0.2f, 0.35f);
        public Color4 Dark2 => getColour(0.2f, 0.3f);
        public Color4 Dark3 => getColour(0.2f, 0.25f);
        public Color4 Dark4 => getColour(0.2f, 0.2f);
        public Color4 Dark5 => getColour(0.2f, 0.15f);
        public Color4 Dark6 => getColour(0.2f, 0.1f);
        public Color4 Foreground1 => getColour(0.1f, 0.6f);
        public Color4 Background1 => getColour(0.1f, 0.4f);
        public Color4 Background2 => getColour(0.1f, 0.3f);
        public Color4 Background3 => getColour(0.1f, 0.25f);
        public Color4 Background4 => getColour(0.1f, 0.2f);
        public Color4 Background5 => getColour(0.1f, 0.15f);
        public Color4 Background6 => getColour(0.1f, 0.1f);
        private Color4 getColour(float saturation, float lightness) => Color4.FromHsl(new Vector4(HueColour, saturation, lightness, 1));
        public float HueColour;

        public CustomColourProvider(float r, float g, float b)
        {
            HueColour = Color4.ToHsl(new Color4(r,g,b,1)).X;
        }

        public void UpdateHueColor(float r, float g, float b)
        {
            HueColour = Color4.ToHsl(new Color4(r,g,b,1)).X;
        }
    }
}