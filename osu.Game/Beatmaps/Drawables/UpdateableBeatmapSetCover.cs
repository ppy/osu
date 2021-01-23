// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
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
                Colour = OsuColour.Gray(0.2f),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateCover();
        }

        protected virtual BeatmapSetCover CreateBeatmapSetCover(BeatmapSetInfo beatmapSet, BeatmapSetCoverType coverType) => new BeatmapSetCover(beatmapSet, coverType);

        private void updateCover()
        {
            displayedCover?.FadeOut(400);
            displayedCover?.Expire();
            displayedCover = null;

            if (beatmapSet != null)
            {
                Add(displayedCover = new DelayedLoadUnloadWrapper(() =>
                {
                    var cover = CreateBeatmapSetCover(beatmapSet, coverType);
                    cover.Anchor = Anchor.Centre;
                    cover.Origin = Anchor.Centre;
                    cover.RelativeSizeAxes = Axes.Both;
                    cover.FillMode = FillMode.Fill;
                    cover.OnLoadComplete += d => d.FadeInFromZero(400, Easing.Out);
                    return cover;
                }));
            }
        }
    }
}
