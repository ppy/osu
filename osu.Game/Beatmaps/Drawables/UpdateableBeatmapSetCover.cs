﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    public class UpdateableBeatmapSetCover : Container
    {
        private Drawable displayedCover;

        private BeatmapSetInfo beatmapSet;
        public BeatmapSetInfo BeatmapSet
        {
            get { return beatmapSet; }
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
            get { return coverType; }
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
                Colour = Color4.Black,
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
                Add(displayedCover = new DelayedLoadWrapper(
                    new BeatmapSetCover(beatmapSet, coverType)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fill,
                        OnLoadComplete = d => d.FadeInFromZero(400, Easing.Out),
                    })
                );
            }
        }
    }
}
