// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Database;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    internal class DifficultyIcon : Container
    {
        private readonly BeatmapInfo beatmap;
        private OsuColour palette;

        public DifficultyIcon(BeatmapInfo beatmap)
        {
            this.beatmap = beatmap;
            const float size = 20;
            Size = new Vector2(size);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour palette)
        {
            this.palette = palette;

            Children = new[]
            {
                new TextAwesome
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    TextSize = Size.X,
                    Colour = getColour(beatmap),
                    Icon = FontAwesome.fa_circle
                },
                new TextAwesome
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    TextSize = Size.X,
                    Colour = Color4.White,
                    Icon = beatmap.Ruleset.CreateInstance().Icon
                }
            };
        }

        private enum DifficultyRating
        {
            Easy,
            Normal,
            Hard,
            Insane,
            Expert
        }

        private DifficultyRating getDifficultyRating(BeatmapInfo beatmap)
        {
            var rating = beatmap.StarDifficulty;

            if (rating < 1.5) return DifficultyRating.Easy;
            if (rating < 2.25) return DifficultyRating.Normal;
            if (rating < 3.75) return DifficultyRating.Hard;
            if (rating < 5.25) return DifficultyRating.Insane;
            return DifficultyRating.Expert;
        }

        private Color4 getColour(BeatmapInfo beatmap)
        {
            switch (getDifficultyRating(beatmap))
            {
                case DifficultyRating.Easy:
                    return palette.Green;
                default:
                case DifficultyRating.Normal:
                    return palette.Yellow;
                case DifficultyRating.Hard:
                    return palette.Pink;
                case DifficultyRating.Insane:
                    return palette.Purple;
                case DifficultyRating.Expert:
                    return palette.Gray0;
            }
        }
    }
}