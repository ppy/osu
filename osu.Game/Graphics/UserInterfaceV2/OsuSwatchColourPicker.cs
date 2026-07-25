// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class OsuSwatchColourPicker : SwatchColourPicker
    {
        private const float swatch_size = 28;

        public OsuSwatchColourPicker(BindableList<Colour4>? colours = null)
            : base(colours)
        {
            Content.Padding = new MarginPadding { Horizontal = 20, Top = 20 };
        }

        [BackgroundDependencyLoader(true)]
        private void load(OverlayColourProvider? overlayColourProvider, OsuColour osuColour)
        {
            Background.Colour = overlayColourProvider?.Dark6 ?? osuColour.GreySeaFoamDarker;
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
