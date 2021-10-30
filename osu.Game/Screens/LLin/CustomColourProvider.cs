// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.LLin
{
    ///<summary>
    ///更改自<see cref="OverlayColourProvider"/>
    ///</summary>
    public class CustomColourProvider : Component
    {
        public Color4 ActiveColor => Highlight2;
        public Color4 InActiveColor => Dark4;
        public Color4 Highlight1 => getColour(1, 0.7f);
        public Color4 Highlight2 => getColour(0.7f, 0.7f);
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
        public Color4 Background7 => getColour(0.1f, 0.05f);
        private Color4 getColour(float saturation, float lightness) => Color4.FromHsl(new Vector4(HueColour.Value, saturation, lightness, 1));
        public BindableFloat HueColour = new BindableFloat();

        private readonly BindableFloat iR = new BindableFloat();
        private readonly BindableFloat iG = new BindableFloat();
        private readonly BindableFloat iB = new BindableFloat();

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            config.BindWith(MSetting.MvisInterfaceRed, iR);
            config.BindWith(MSetting.MvisInterfaceGreen, iG);
            config.BindWith(MSetting.MvisInterfaceBlue, iB);

            iR.BindValueChanged(_ => updateColor());
            iG.BindValueChanged(_ => updateColor());
            iB.BindValueChanged(_ => updateColor(), true);
        }

        private void updateColor() => UpdateHueColor(iR.Value, iG.Value, iB.Value);

        public void UpdateHueColor(float r, float g, float b)
        {
            HueColour.Value = Color4.ToHsl(new Color4(r, g, b, 1)).X;
        }
    }
}
