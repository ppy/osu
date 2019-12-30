// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;

namespace osu.Game.Overlays
{
    public abstract class WebOverlay : FullscreenOverlay
    {
        protected readonly OverlayColourScheme ColourScheme;

        protected override Container<Drawable> Content => content;

        private readonly Box background;
        private readonly Container content;

        public WebOverlay(OverlayColourScheme colourScheme)
        {
            ColourScheme = colourScheme;

            base.Content.AddRange(new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Waves.FirstWaveColour = colours.ForOverlayElement(ColourScheme, 0.6f, 0.5f);
            Waves.SecondWaveColour = colours.ForOverlayElement(ColourScheme, 0.5f, 0.3f);
            Waves.ThirdWaveColour = colours.ForOverlayElement(ColourScheme, 0.4f, 0.4f);
            Waves.FourthWaveColour = colours.ForOverlayElement(ColourScheme, 0.3f, 0.15f);

            background.Colour = colours.ForOverlayElement(ColourScheme, 0.1f, 0.15f);
        }
    }
}
