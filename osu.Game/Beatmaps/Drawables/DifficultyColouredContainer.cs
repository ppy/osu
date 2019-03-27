// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    public abstract class DifficultyColouredContainer : Container, IHasAccentColour
    {
        public Color4 AccentColour { get; set; }

        private readonly BeatmapInfo beatmap;
        private OsuColour palette;

        protected DifficultyColouredContainer(BeatmapInfo beatmap)
        {
            this.beatmap = beatmap;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour palette)
        {
            if (palette == null)
                throw new ArgumentNullException(nameof(palette));

            this.palette = palette;
            AccentColour = getColour(beatmap);
        }

        private enum DifficultyRating
        {
            Easy,
            Normal,
            Hard,
            Insane,
            Expert,
            ExpertPlus
        }

        private DifficultyRating getDifficultyRating(BeatmapInfo beatmap)
        {
            if (beatmap == null)
                throw new ArgumentNullException(nameof(beatmap));

            var rating = beatmap.StarDifficulty;

            if (rating < 2.0) return DifficultyRating.Easy;
            if (rating < 2.7) return DifficultyRating.Normal;
            if (rating < 4.0) return DifficultyRating.Hard;
            if (rating < 5.3) return DifficultyRating.Insane;
            if (rating < 6.5) return DifficultyRating.Expert;

            return DifficultyRating.ExpertPlus;
        }

        private Color4 getColour(BeatmapInfo beatmap)
        {
            switch (getDifficultyRating(beatmap))
            {
                case DifficultyRating.Easy:
                    return palette.Green;
                default:
                case DifficultyRating.Normal:
                    return palette.Blue;
                case DifficultyRating.Hard:
                    return palette.Yellow;
                case DifficultyRating.Insane:
                    return palette.Pink;
                case DifficultyRating.Expert:
                    return palette.Purple;
                case DifficultyRating.ExpertPlus:
                    return palette.Gray0;
            }
        }
    }
}
