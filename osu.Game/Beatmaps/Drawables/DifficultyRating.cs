// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Beatmaps.Drawables
{
    public enum DifficultyType
    {
        Easy,
        Normal,
        Hard,
        Insane,
        Expert,
        ExpertPlus
    }

    public static class DifficultyRating
    {
        public static DifficultyType GetDifficultyType(double starDifficulty)
        {
            if (starDifficulty < 2.0) return DifficultyType.Easy;
            if (starDifficulty < 2.7) return DifficultyType.Normal;
            if (starDifficulty < 4.0) return DifficultyType.Hard;
            if (starDifficulty < 5.3) return DifficultyType.Insane;
            if (starDifficulty < 6.5) return DifficultyType.Expert;

            return DifficultyType.ExpertPlus;
        }

        public static DifficultyType GetDifficultyType(BeatmapInfo beatmap) => GetDifficultyType(beatmap.StarDifficulty);
    }
}
