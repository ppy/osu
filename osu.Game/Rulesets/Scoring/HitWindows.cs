// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        /// An empty <see cref="HitWindows"/> with only <see cref="HitResult.Miss"/> and <see cref="HitResult.Perfect"/>.
        /// No time values are provided (meaning instantaneous hit or miss).
        /// </summary>
        public static HitWindows Empty => new EmptyHitWindows();

        public HitWindows()
        {
            Debug.Assert(GetRanges().Any(r => r.Result == HitResult.Miss), $"{nameof(GetRanges)} should always contain {nameof(HitResult.Miss)}");
            Debug.Assert(GetRanges().Any(r => r.Result != HitResult.Miss), $"{nameof(GetRanges)} should always contain at least one result type other than {nameof(HitResult.Miss)}.");
        }

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
                double value = IBeatmapDifficultyInfo.DifficultyRange(difficulty, (range.Min, range.Average, range.Max));

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
                    throw new ArgumentException("Unknown enum member", nameof(result));
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

        public class EmptyHitWindows : HitWindows
        {
            private static readonly DifficultyRange[] ranges =
            {
                new DifficultyRange(HitResult.Perfect, 0, 0, 0),
                new DifficultyRange(HitResult.Miss, 0, 0, 0),
            };

            public override bool IsHitResultAllowed(HitResult result)
            {
                switch (result)
                {
                    case HitResult.Perfect:
                    case HitResult.Miss:
                        return true;
                }

                return false;
            }

            protected override DifficultyRange[] GetRanges() => ranges;
        }
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
