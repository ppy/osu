﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Scoring
{
    /// <summary>
    /// A structure containing timing data for hit window based gameplay.
    /// </summary>
    public class HitWindows
    {
        private static readonly DifficultyRange[] base_ranges =
        {
            new DifficultyRange(HitResult.Perfect, 22.4D, 19.4D, 13.9D),
            new DifficultyRange(HitResult.Great, 64, 49, 34),
            new DifficultyRange(HitResult.Good, 97, 82, 67),
            new DifficultyRange(HitResult.Ok, 127, 112, 97),
            new DifficultyRange(HitResult.Meh, 151, 136, 121),
            new DifficultyRange(HitResult.Miss, 188, 173, 158),
        };

        private double perfect;
        private double great;
        private double good;
        private double ok;
        private double meh;
        private double miss;

        /// <summary>
        /// Retrieves the <see cref="HitResult"/> with the largest hit window that produces a successful hit.
        /// </summary>
        /// <returns>The lowest allowed successful <see cref="HitResult"/>.</returns>
        protected HitResult LowestSuccessfulHitResult()
        {
            for (var result = HitResult.Meh; result <= HitResult.Perfect; ++result)
            {
                if (IsHitResultAllowed(result))
                    return result;
            }

            return HitResult.None;
        }

        /// <summary>
        /// Retrieves a mapping of <see cref="HitResult"/>s to their timing windows for all allowed <see cref="HitResult"/>s.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<(HitResult result, double length)> GetAllAvailableWindows()
        {
            for (var result = HitResult.Meh; result <= HitResult.Perfect; ++result)
            {
                if (IsHitResultAllowed(result))
                    yield return (result, WindowFor(result));
            }
        }

        /// <summary>
        /// Check whether it is possible to achieve the provided <see cref="HitResult"/>.
        /// </summary>
        /// <param name="result">The result type to check.</param>
        /// <returns>Whether the <see cref="HitResult"/> can be achieved.</returns>
        public virtual bool IsHitResultAllowed(HitResult result) => true;

        /// <summary>
        /// Sets hit windows with values that correspond to a difficulty parameter.
        /// </summary>
        /// <param name="difficulty">The parameter.</param>
        public void SetDifficulty(double difficulty)
        {
            foreach (var range in GetRanges())
            {
                var value = BeatmapDifficulty.DifficultyRange(difficulty, (range.Min, range.Average, range.Max));

                switch (range.Result)
                {
                    case HitResult.Miss:
                        miss = value;
                        break;

                    case HitResult.Meh:
                        meh = value;
                        break;

                    case HitResult.Ok:
                        ok = value;
                        break;

                    case HitResult.Good:
                        good = value;
                        break;

                    case HitResult.Great:
                        great = value;
                        break;

                    case HitResult.Perfect:
                        perfect = value;
                        break;
                }
            }
        }

        /// <summary>
        /// Retrieves the <see cref="HitResult"/> for a time offset.
        /// </summary>
        /// <param name="timeOffset">The time offset.</param>
        /// <returns>The hit result, or <see cref="HitResult.None"/> if <paramref name="timeOffset"/> doesn't result in a judgement.</returns>
        public HitResult ResultFor(double timeOffset)
        {
            timeOffset = Math.Abs(timeOffset);

            for (var result = HitResult.Perfect; result >= HitResult.Miss; --result)
            {
                if (IsHitResultAllowed(result) && timeOffset <= WindowFor(result))
                    return result;
            }

            return HitResult.None;
        }

        /// <summary>
        /// Retrieves the hit window for a <see cref="HitResult"/>.
        /// This is the number of +/- milliseconds allowed for the requested result (so the actual hittable range is double this).
        /// </summary>
        /// <param name="result">The expected <see cref="HitResult"/>.</param>
        /// <returns>One half of the hit window for <paramref name="result"/>.</returns>
        public double WindowFor(HitResult result)
        {
            switch (result)
            {
                case HitResult.Perfect:
                    return perfect;

                case HitResult.Great:
                    return great;

                case HitResult.Good:
                    return good;

                case HitResult.Ok:
                    return ok;

                case HitResult.Meh:
                    return meh;

                case HitResult.Miss:
                    return miss;

                default:
                    throw new ArgumentException(nameof(result));
            }
        }

        /// <summary>
        /// Given a time offset, whether the <see cref="HitObject"/> can ever be hit in the future with a non-<see cref="HitResult.Miss"/> result.
        /// This happens if <paramref name="timeOffset"/> is less than what is required for <see cref="LowestSuccessfulHitResult"/>.
        /// </summary>
        /// <param name="timeOffset">The time offset.</param>
        /// <returns>Whether the <see cref="HitObject"/> can be hit at any point in the future from this time offset.</returns>
        public bool CanBeHit(double timeOffset) => timeOffset <= WindowFor(LowestSuccessfulHitResult());

        /// <summary>
        /// Retrieve a valid list of <see cref="DifficultyRange"/>s representing hit windows.
        /// Defaults are provided but can be overridden to customise for a ruleset.
        /// </summary>
        protected virtual DifficultyRange[] GetRanges() => base_ranges;
    }

    public struct DifficultyRange
    {
        public readonly HitResult Result;

        public double Min;
        public double Average;
        public double Max;

        public DifficultyRange(HitResult result, double min, double average, double max)
        {
            Result = result;

            Min = min;
            Average = average;
            Max = max;
        }
    }
}
