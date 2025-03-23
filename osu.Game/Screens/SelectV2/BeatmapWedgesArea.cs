// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.SelectV2.Wedges;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapWedgesArea : VisibilityContainer
    {
        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        private BeatmapWedgesHeader header = null!;
        private Container contentContainer = null!;

        public BeatmapWedgesArea()
        {
            RelativeSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            CornerRadius = 10;
            Masking = true;

            const float header_height = 48f;

            InternalChildren = new Drawable[]
            {
                new ShearAlignedDrawable(shear, header = new BeatmapWedgesHeader
                {
                    RelativeSizeAxes = Axes.X,
                    Height = header_height,
                }),
                new Container
                {
                    Depth = 1f,
                    Padding = new MarginPadding { Top = header_height },
                    RelativeSizeAxes = Axes.Both,
                    Child = new ShearAlignedDrawable(shear, contentContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                    }),
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            header.Type.BindValueChanged(_ => updateDisplay(), true);
        }

        protected override void PopIn()
        {
            this.MoveToX(0, SongSelect.ENTER_DURATION, Easing.OutQuint)
                .FadeIn(SongSelect.ENTER_DURATION / 3, Easing.In);
        }

        protected override void PopOut()
        {
            this.MoveToX(-150, SongSelect.ENTER_DURATION, Easing.OutQuint)
                .FadeOut(SongSelect.ENTER_DURATION / 3, Easing.In);
        }

        private Drawable? currentContent;

        private void updateDisplay()
        {
            if (currentContent != null)
            {
                currentContent.Hide();
                currentContent.Expire();
            }

            switch (header.Type.Value)
            {
                default:
                case BeatmapWedgesHeader.Selection.Details:
                    currentContent = new BeatmapDetailsWedge();
                    break;

                case BeatmapWedgesHeader.Selection.Ranking:
                    currentContent = new BeatmapRankingsWedge
                    {
                        Scope = { BindTarget = header.Scope },
                        FilterBySelectedMods = { BindTarget = header.FilterBySelectedMods },
                    };

                    break;
            }

            contentContainer.Add(currentContent);
            currentContent.Show();
        }
    }
}
