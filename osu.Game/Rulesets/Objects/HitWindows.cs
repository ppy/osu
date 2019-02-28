// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Objects
{
    public class HitWindows
    {
        private static readonly IReadOnlyDictionary<HitResult, (double od0, double od5, double od10)> base_ranges = new Dictionary<HitResult, (double, double, double)>
        {
            { HitResult.Perfect, (44.8, 38.8, 27.8) },
            { HitResult.Great, (128, 98, 68) },
            { HitResult.Good, (194, 164, 134) },
            { HitResult.Ok, (254, 224, 194) },
            { HitResult.Meh, (302, 272, 242) },
            { HitResult.Miss, (376, 346, 316) },
        };

        /// <summary>
        /// Hit window for a <see cref="HitResult.Perfect"/> result.
        /// </summary>
        public double Perfect { get; protected set; }

        /// <summary>
        /// Hit window for a <see cref="HitResult.Great"/> result.
        /// </summary>
        public double Great { get; protected set; }

        /// <summary>
        /// Hit window for a <see cref="HitResult.Good"/> result.
        /// </summary>
        public double Good { get; protected set; }

        /// <summary>
        /// Hit window for an <see cref="HitResult.Ok"/> result.
        /// </summary>
        public double Ok { get; protected set; }

        /// <summary>
        /// Hit window for a <see cref="HitResult.Meh"/> result.
        /// </summary>
        public double Meh { get; protected set; }

        /// <summary>
        /// Hit window for a <see cref="HitResult.Miss"/> result.
        /// </summary>
        public double Miss { get; protected set; }

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
        /// Check whether it is possible to achieve the provided <see cref="HitResult"/>.
        /// </summary>
        /// <param name="result">The result type to check.</param>
        /// <returns>Whether the <see cref="HitResult"/> can be achieved.</returns>
        public virtual bool IsHitResultAllowed(HitResult result)
        {
            switch (result)
            {
                case HitResult.Perfect:
                case HitResult.Ok:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Sets hit windows with values that correspond to a difficulty parameter.
        /// </summary>
        /// <param name="difficulty">The parameter.</param>
        public virtual void SetDifficulty(double difficulty)
        {
            Perfect = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Perfect]);
            Great = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Great]);
            Good = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Good]);
            Ok = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Ok]);
            Meh = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Meh]);
            Miss = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Miss]);
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
                if (IsHitResultAllowed(result) && timeOffset <= HalfWindowFor(result))
                    return result;
            }

            return HitResult.None;
        }

        /// <summary>
        /// Retrieves half the hit window for a <see cref="HitResult"/>.
        /// This is useful if the hit window for one half of the hittable range of a <see cref="HitObject"/> is required.
        /// </summary>
        /// <param name="result">The expected <see cref="HitResult"/>.</param>
        /// <returns>One half of the hit window for <paramref name="result"/>.</returns>
        public double HalfWindowFor(HitResult result)
        {
            switch (result)
            {
                case HitResult.Perfect:
                    return Perfect / 2;
                case HitResult.Great:
                    return Great / 2;
                case HitResult.Good:
                    return Good / 2;
                case HitResult.Ok:
                    return Ok / 2;
                case HitResult.Meh:
                    return Meh / 2;
                case HitResult.Miss:
                    return Miss / 2;
                default:
                    throw new ArgumentException(nameof(result));
            }
        }

        /// <summary>
        /// Given a time offset, whether the <see cref="HitObject"/> can ever be hit in the future with a non-<see cref="HitResult.Miss"/> result.
        /// This happens if <paramref name="timeOffset"/> is less than what is required for a <see cref="SuccessfulHitWindow"/> result.
        /// </summary>
        /// <param name="timeOffset">The time offset.</param>
        /// <returns>Whether the <see cref="HitObject"/> can be hit at any point in the future from this time offset.</returns>
        public bool CanBeHit(double timeOffset) => timeOffset <= HalfWindowFor(LowestSuccessfulHitResult());
    }
}
