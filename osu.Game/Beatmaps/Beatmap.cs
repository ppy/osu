// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Game.Database;
using osu.Game.Modes;
using osu.Game.Modes.Objects;
using System;
using System.Collections.Generic;

namespace osu.Game.Beatmaps
{
    public class Beatmap
    {
        public BeatmapInfo BeatmapInfo { get; set; }
        public BeatmapMetadata Metadata => BeatmapInfo?.Metadata ?? BeatmapInfo?.BeatmapSet?.Metadata;
        public List<HitObject> HitObjects { get; set; }
        public readonly TimingInfo Timing = new TimingInfo();
        public List<Color4> ComboColors { get; set; }

        public double CalculateStarDifficulty() => Ruleset.GetRuleset(BeatmapInfo.Mode).CreateDifficultyCalculator(this).Calculate();

        /// <summary>
        /// Finds the slider velocity (in distance units per second) at a time.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns>The slider velocity.</returns>
        public double SliderVelocityAt(double time)
        {
            double scoringDistance = 100 * BeatmapInfo.BaseDifficulty.SliderMultiplier;
            double beatDistance = Timing.BeatDistanceAt(time);

            if (beatDistance > 0)
                return scoringDistance / beatDistance * 1000;
            return scoringDistance;
        }

        /// <summary>
        /// Applies the mods Easy and HardRock to the provided difficulty value.
        /// </summary>
        /// <param name="difficulty">Difficulty value to be modified.</param>
        /// <param name="hardRockFactor">Factor by which HardRock increases difficulty.</param>
        /// <param name="mods">Mods to be applied.</param>
        /// <returns>Modified difficulty value.</returns>
        public static double ApplyModsToDifficulty(double difficulty, double hardRockFactor, Mods mods)
        {
            if ((mods & Mods.Easy) > 0)
                difficulty = Math.Max(0, difficulty / 2);
            if ((mods & Mods.HardRock) > 0)
                difficulty = Math.Min(10, difficulty * hardRockFactor);

            return difficulty;
        }

        /// <summary>
        /// Maps a difficulty value [0, 10] to a range of resulting values with respect to currently active mods.
        /// </summary>
        /// <param name="difficulty">The difficulty value to be mapped.</param>
        /// <param name="min">Minimum of the resulting range which will be achieved by a difficulty value of 0.</param>
        /// <param name="mid">Midpoint of the resulting range which will be achieved by a difficulty value of 5.</param>
        /// <param name="max">Maximum of the resulting range which will be achieved by a difficulty value of 10.</param>
        /// <returns>Value to which the difficulty value maps in the specified range.</returns>
        public static double MapDifficultyRange(double difficulty, double min, double mid, double max, Mods mods)
        {
            difficulty = ApplyModsToDifficulty(difficulty, 1.4, mods);

            if (difficulty > 5)
                return mid + (max - mid) * (difficulty - 5) / 5;
            if (difficulty < 5)
                return mid - (mid - min) * (5 - difficulty) / 5;
            return mid;
        }
    }

    public enum Mods
    {
        None,
        Easy,
        HardRock
    }
}
