// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class OsuSwatchColourPicker : SwatchColourPicker
    {
        private const float swatch_size = 28;

        public LocalisableString PaletteHeaderText { get; init; } = string.Empty;

        public OsuSwatchColourPicker()
        {
            Content.Padding = new MarginPadding { Horizontal = 20, Top = 20 };
        }

        [BackgroundDependencyLoader(true)]
        private void load(OverlayColourProvider? overlayColourProvider, OsuColour osuColour)
        {
            Background.Colour = overlayColourProvider?.Dark6 ?? osuColour.GreySeaFoamDarker;

            if (!string.IsNullOrEmpty(PaletteHeaderText.ToString()))
            {
                OsuTextFlowContainer text;
                AddInternal(text = new OsuTextFlowContainer(c => c.Font = OsuFont.Style.Caption1)
                {
                    Padding = new MarginPadding { Horizontal = 20, Top = 20 },
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = PaletteHeaderText,
                });

                Content.Y += text.DrawHeight;
            }
        }

        protected override ClickableContainer CreateSwatch(Colour4 colour) => new OsuSwatch(colour);

        private partial class OsuSwatch : OsuClickableContainer
        {
            public OsuSwatch(Colour4 colour)
            {
                Size = new Vector2(swatch_size);
                Masking = true;
                CornerRadius = 5;
                TooltipText = colour.ToHex();

                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colour,
                };
            }
        }
    }
}
