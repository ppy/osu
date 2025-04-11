// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    /// <summary>
    /// The left portion of the song select screen which houses the metadata or leaderboards wedge, along with controls
    /// to switch between them and adjust specifics.
    /// </summary>
    public partial class BeatmapDetailsArea : VisibilityContainer
    {
        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        private Header header = null!;
        private Container contentContainer = null!;

        public BeatmapDetailsArea()
        {
            RelativeSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            const float header_height = 35f;

            InternalChildren = new Drawable[]
            {
                new ShearAlignedDrawable(shear, header = new Header
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
                case Header.Selection.Details:
                    currentContent = new BeatmapMetadataWedge();
                    break;

                case Header.Selection.Ranking:
                    currentContent = new BeatmapLeaderboardWedge
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
