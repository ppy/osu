// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapWedgesHeader : CompositeDrawable
    {
        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        private BeatmapWedgesTabControl<Selection> tabControl = null!;

        public IBindable<Selection> Type => tabControl.Current;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Shear = shear;
            CornerRadius = 10;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Shear = -shear,
                    Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN, Right = 20f },
                    Children = new Drawable[]
                    {
                        tabControl = new BeatmapWedgesTabControl<Selection>(20f)
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Width = 200,
                            Height = 22,
                            Margin = new MarginPadding { Top = 2f },
                        },
                    },
                },
            };
        }

        public enum Selection
        {
            Details,
        }
    }
}
