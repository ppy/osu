// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Scoring
{
    [HasOrderedElements]
    public enum HitResult
    {
        /// <summary>
        /// Indicates that the object has not been judged yet.
        /// </summary>
        [Description(@"")]
        [Order(13)]
        None,

        [Order(12)]
        Ignore,

        /// <summary>
        /// Indicates that the object has been judged as a miss.
        /// </summary>
        /// <remarks>
        /// This miss window should determine how early a hit can be before it is considered for judgement (as opposed to being ignored as
        /// "too far in the future). It should also define when a forced miss should be triggered (as a result of no user input in time).
        /// </remarks>
        [Description(@"Miss")]
        [Order(5)]
        Miss,

        [Description(@"Meh")]
        [Order(4)]
        Meh,

        /// <summary>
        /// Optional judgement.
        /// </summary>
        [Description(@"OK")]
        [Order(3)]
        Ok,

        [Description(@"Good")]
        [Order(2)]
        Good,

        [Description(@"Great")]
        [Order(1)]
        Great,

        /// <summary>
        /// Optional judgement.
        /// </summary>
        [Description(@"Perfect")]
        [Order(0)]
        Perfect,

        /// <summary>
        /// Indicates small tick miss.
        /// </summary>
        [Order(11)]
        SmallTickMiss,

        /// <summary>
        /// Indicates a small tick hit.
        /// </summary>
        [Description(@"S Tick")]
        [Order(7)]
        SmallTickHit,

        /// <summary>
        /// Indicates a large tick miss.
        /// </summary>
        [Order(10)]
        LargeTickMiss,

        /// <summary>
        /// Indicates a large tick hit.
        /// </summary>
        [Description(@"L Tick")]
        [Order(6)]
        LargeTickHit,

        /// <summary>
        /// Indicates a small bonus.
        /// </summary>
        [Description("S Bonus")]
        [Order(9)]
        SmallBonus,

        /// <summary>
        /// Indicate a large bonus.
        /// </summary>
        [Description("L Bonus")]
        [Order(8)]
        LargeBonus,
    }

    public static class HitResultExtensions
    {
        /// <summary>
        /// Whether a <see cref="HitResult"/> affects the combo.
        /// </summary>
        public static bool AffectsCombo(this HitResult result)
        {
            switch (result)
            {
                case HitResult.Miss:
                case HitResult.Meh:
                case HitResult.Ok:
                case HitResult.Good:
                case HitResult.Great:
                case HitResult.Perfect:
                case HitResult.LargeTickHit:
                case HitResult.LargeTickMiss:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Whether a <see cref="HitResult"/> should be counted as combo score.
        /// </summary>
        /// <remarks>
        /// This is not the reciprocal of <see cref="AffectsCombo"/>, as <see cref="HitResult.SmallTickHit"/> and <see cref="HitResult.SmallTickMiss"/> do not affect combo
        /// but are still considered as part of the accuracy (not bonus) portion of the score.
        /// </remarks>
        public static bool IsBonus(this HitResult result)
        {
            switch (result)
            {
                case HitResult.SmallBonus:
                case HitResult.LargeBonus:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Whether a <see cref="HitResult"/> represents a successful hit.
        /// </summary>
        public static bool IsHit(this HitResult result)
        {
            switch (result)
            {
                case HitResult.None:
                case HitResult.Ignore:
                case HitResult.Miss:
                case HitResult.SmallTickMiss:
                case HitResult.LargeTickMiss:
                    return false;

                default:
                    return true;
            }
        }

        /// <summary>
        /// Whether a <see cref="HitResult"/> is scorable.
        /// </summary>
        public static bool IsScorable(this HitResult result) => result > HitResult.Ignore;
    }
}
