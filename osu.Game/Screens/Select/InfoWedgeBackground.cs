// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;

namespace osu.Game.Screens.Select
{
    public partial class InfoWedgeBackground : Container
    {
        private readonly Container content = new Container
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
        };

        protected override Container<Drawable> Content => content;

        public InfoWedgeBackground()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding
            {
                Top = 10,
                Left = -SongSelect.WEDGE_CORNER_RADIUS,

                // TODO: should account top wedge's shear width for alignment (hard to do as this auto-sizes height right now)
                Right = BeatmapInfoWedgeV2.SHEAR_WIDTH + BeatmapInfoWedgeV2.COLOUR_BAR_WIDTH
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = SongSelect.WEDGE_CORNER_RADIUS,
                    Shear = SongSelect.WEDGED_CONTAINER_SHEAR,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background5,
                        },
                    },
                },
                content
            };
        }
    }
}
