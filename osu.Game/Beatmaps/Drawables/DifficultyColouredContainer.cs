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

        private Color4 getColour(BeatmapInfo beatmap)
        {
            switch (DifficultyRating.GetDifficultyType(beatmap))
            {
                case DifficultyType.Easy:
                    return palette.Green;
                    
                case DifficultyType.Normal:
                    return palette.Blue;

                case DifficultyType.Hard:
                    return palette.Yellow;

                case DifficultyType.Insane:
                    return palette.Pink;

                case DifficultyType.Expert:
                    return palette.Purple;

                case DifficultyType.ExpertPlus:
                    return palette.Gray0;

                default:
                    return Color4.Black;
            }
        }
    }
}
