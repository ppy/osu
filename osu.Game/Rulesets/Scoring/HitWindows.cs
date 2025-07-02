// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Scoring
{
    /// <summary>
    /// A structure containing timing data for hit window based gameplay.
    /// </summary>
    public abstract class HitWindows
    {
        /// <summary>
        /// An empty <see cref="HitWindows"/> with only <see cref="HitResult.Miss"/> and <see cref="HitResult.Perfect"/>.
        /// No time values are provided (meaning instantaneous hit or miss).
        /// </summary>
        public static HitWindows Empty { get; } = new EmptyHitWindows();

        protected HitWindows()
        {
            ensureValidHitWindows();
        }

        [Conditional("DEBUG")]
        private void ensureValidHitWindows()
        {
            var availableWindows = GetAllAvailableWindows().ToList();
            Debug.Assert(availableWindows.Any(r => r.result == HitResult.Miss), $"{nameof(GetAllAvailableWindows)} should always contain {nameof(HitResult.Miss)}");
            Debug.Assert(availableWindows.Any(r => r.result != HitResult.Miss),
                $"{nameof(GetAllAvailableWindows)} should always contain at least one result type other than {nameof(HitResult.Miss)}.");
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
            for (var result = HitResult.Miss; result <= HitResult.Perfect; ++result)
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
        public abstract void SetDifficulty(double difficulty);

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
        public abstract double WindowFor(HitResult result);

        /// <summary>
        /// Given a time offset, whether the <see cref="HitObject"/> can ever be hit in the future with a non-<see cref="HitResult.Miss"/> result.
        /// This happens if <paramref name="timeOffset"/> is less than what is required for <see cref="LowestSuccessfulHitResult"/>.
        /// </summary>
        /// <param name="timeOffset">The time offset.</param>
        /// <returns>Whether the <see cref="HitObject"/> can be hit at any point in the future from this time offset.</returns>
        public bool CanBeHit(double timeOffset) => timeOffset <= WindowFor(LowestSuccessfulHitResult());

        private class EmptyHitWindows : HitWindows
        {
            public override bool IsHitResultAllowed(HitResult result) => true;

            public override void SetDifficulty(double difficulty) { }

            public override double WindowFor(HitResult result) => 0;
        }
    }
}
