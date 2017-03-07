// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics;
using osu.Game.Beatmaps.Timing;
using osu.Game.Database;
using osu.Game.Modes.Objects;
using osu.Game.Modes;
using System;

namespace osu.Game.Beatmaps
{
    public class Beatmap
    {
        public BeatmapInfo BeatmapInfo { get; set; }
        public BeatmapMetadata Metadata => BeatmapInfo?.Metadata ?? BeatmapInfo?.BeatmapSet?.Metadata;
        public List<HitObject> HitObjects { get; set; }
        public List<ControlPoint> ControlPoints { get; set; }
        public List<Color4> ComboColors { get; set; }
        public double BPMMaximum => 60000 / (ControlPoints?.Where(c => c.BeatLength != 0).OrderBy(c => c.BeatLength).FirstOrDefault() ?? ControlPoint.Default).BeatLength;
        public double BPMMinimum => 60000 / (ControlPoints?.Where(c => c.BeatLength != 0).OrderByDescending(c => c.BeatLength).FirstOrDefault() ?? ControlPoint.Default).BeatLength;
        public double BPMMode => BPMAt(ControlPoints.Where(c => c.BeatLength != 0).GroupBy(c => c.BeatLength).OrderByDescending(grp => grp.Count()).First().First().Time);

        public double BPMAt(double time)
        {
            return 60000 / BeatLengthAt(time);
        }

        /// <summary>
        /// Finds the BPM multiplier at a time.
        /// </summary>
        /// <param name="time">The time to find the BPM multiplier at.</param>
        /// <returns>The BPM multiplier.</returns>
        public double BPMMultiplierAt(double time)
        {
            ControlPoint overridePoint;
            ControlPoint timingPoint = TimingPointAt(time, out overridePoint);

            return overridePoint?.VelocityAdjustment ?? 1;
        }

        /// <summary>
        /// Finds the beat length (in milliseconds) at a time.
        /// </summary>
        /// <param name="time">The time to find the beat length at.</param>
        /// <returns>The beat length.</returns>
        public double BeatLengthAt(double time)
        {
            ControlPoint overridePoint;
            ControlPoint timingPoint = TimingPointAt(time, out overridePoint);

            return timingPoint.BeatLength;
        }

        /// <summary>
        /// Finds the beat velocity at a time.
        /// </summary>
        /// <param name="time">The time to find the velocity at.</param>
        /// <returns>The velocity.</returns>
        public double BeatVelocityAt(double time)
        {
            ControlPoint overridePoint;
            TimingPointAt(time, out overridePoint);

            return overridePoint?.VelocityAdjustment ?? 1;
        }

        /// <summary>
        /// Finds the beat length (in distance) at a time.
        /// </summary>
        /// <param name="time">The time to find the beat length at.</param>
        /// <returns>The beat length.</returns>
        public double BeatDistanceAt(double time)
        {
            ControlPoint overridePoint;
            ControlPoint timingPoint = TimingPointAt(time, out overridePoint);

            return timingPoint.BeatLength * (overridePoint?.VelocityAdjustment ?? 1);
        }

        /// <summary>
        /// Finds the timing point at a time.
        /// </summary>
        /// <param name="time">The time to find the timing point at.</param>
        /// <param name="overridePoint">The timing point containing the velocity change of the returned timing point.</param>
        /// <returns>The timing point.</returns>
        public ControlPoint TimingPointAt(double time, out ControlPoint overridePoint)
        {
            overridePoint = null;

            ControlPoint timingPoint = null;
            foreach (var controlPoint in ControlPoints)
            {
                // Some beatmaps have the first timingPoint (accidentally) start after the first HitObject(s).
                // This null check makes it so that the first ControlPoint that makes a timing change is used as
                // the timingPoint for those HitObject(s).
                if (controlPoint.Time <= time || timingPoint == null)
                {
                    if (controlPoint.TimingChange)
                    {
                        timingPoint = controlPoint;
                        overridePoint = null;
                    }
                    else
                        overridePoint = controlPoint;
                }
                else break;
            }

            return timingPoint ?? ControlPoint.Default;
        }

        public double CalculateStarDifficulty() => Ruleset.GetRuleset(BeatmapInfo.Mode).CreateDifficultyCalculator(this).Calculate();

        /// <summary>
        /// Finds the slider velocity at a time.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns>The slider velocity</returns>
        public double SliderVelocityAt(double time)
        {
            double scoringDistance = 100 * BeatmapInfo.BaseDifficulty.SliderMultiplier;
            double beatDistance = BeatDistanceAt(time);

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
