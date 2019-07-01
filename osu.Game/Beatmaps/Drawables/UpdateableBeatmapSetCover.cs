// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    public class UpdateableBeatmapSetCover : Container
    {
        private Drawable displayedCover;

        private BeatmapSetInfo beatmapSet;

        public BeatmapSetInfo BeatmapSet
        {
            get => beatmapSet;
            set
            {
                if (value == beatmapSet) return;

                beatmapSet = value;

                if (IsLoaded)
                    updateCover();
            }
        }

        private BeatmapSetCoverType coverType = BeatmapSetCoverType.Cover;

        public BeatmapSetCoverType CoverType
        {
            get => coverType;
            set
            {
                if (value == coverType) return;

                coverType = value;

                if (IsLoaded)
                    updateCover();
            }
        }

        public UpdateableBeatmapSetCover()
        {
            Child = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = ColourInfo.GradientVertical(OsuColour.Gray(0.2f), OsuColour.Gray(0.1f)),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateCover();
        }

        private void updateCover()
        {
            displayedCover?.FadeOut(400);
            displayedCover?.Expire();
            displayedCover = null;

            if (beatmapSet != null)
            {
                BeatmapSetCover cover;

                Add(displayedCover = new DelayedLoadWrapper(
                    cover = new BeatmapSetCover(beatmapSet, coverType)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fill,
                    })
                );

                cover.OnLoadComplete += d => d.FadeInFromZero(400, Easing.Out);
            }
        }
    }
}
