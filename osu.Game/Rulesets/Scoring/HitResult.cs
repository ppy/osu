// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Utils;

namespace osu.Game.Rulesets.Scoring
{
    [HasOrderedElements]
    public enum HitResult
    {
        /// <summary>
        /// Indicates that the object has not been judged yet.
        /// </summary>
        [Description(@"")]
        [EnumMember(Value = "none")]
        [Order(14)]
        None,

        /// <summary>
        /// Indicates that the object has been judged as a miss.
        /// </summary>
        /// <remarks>
        /// This miss window should determine how early a hit can be before it is considered for judgement (as opposed to being ignored as
        /// "too far in the future). It should also define when a forced miss should be triggered (as a result of no user input in time).
        /// </remarks>
        [Description(@"Miss")]
        [EnumMember(Value = "miss")]
        [Order(5)]
        Miss,

        [Description(@"Meh")]
        [EnumMember(Value = "meh")]
        [Order(4)]
        Meh,

        [Description(@"OK")]
        [EnumMember(Value = "ok")]
        [Order(3)]
        Ok,

        [Description(@"Good")]
        [EnumMember(Value = "good")]
        [Order(2)]
        Good,

        [Description(@"Great")]
        [EnumMember(Value = "great")]
        [Order(1)]
        Great,

        [Description(@"Perfect")]
        [EnumMember(Value = "perfect")]
        [Order(0)]
        Perfect,

        /// <summary>
        /// Indicates small tick miss.
        /// </summary>
        [EnumMember(Value = "small_tick_miss")]
        [Order(11)]
        SmallTickMiss,

        /// <summary>
        /// Indicates a small tick hit.
        /// </summary>
        [Description(@"S Tick")]
        [EnumMember(Value = "small_tick_hit")]
        [Order(7)]
        SmallTickHit,

        /// <summary>
        /// Indicates a large tick miss.
        /// </summary>
        [EnumMember(Value = "large_tick_miss")]
        [Order(10)]
        LargeTickMiss,

        /// <summary>
        /// Indicates a large tick hit.
        /// </summary>
        [Description(@"L Tick")]
        [EnumMember(Value = "large_tick_hit")]
        [Order(6)]
        LargeTickHit,

        /// <summary>
        /// Indicates a small bonus.
        /// </summary>
        [Description("S Bonus")]
        [EnumMember(Value = "small_bonus")]
        [Order(9)]
        SmallBonus,

        /// <summary>
        /// Indicates a large bonus.
        /// </summary>
        [Description("L Bonus")]
        [EnumMember(Value = "large_bonus")]
        [Order(8)]
        LargeBonus,

        /// <summary>
        /// Indicates a miss that should be ignored for scoring purposes.
        /// </summary>
        [EnumMember(Value = "ignore_miss")]
        [Order(13)]
        IgnoreMiss,

        /// <summary>
        /// Indicates a hit that should be ignored for scoring purposes.
        /// </summary>
        [EnumMember(Value = "ignore_hit")]
        [Order(12)]
        IgnoreHit,

        /// <summary>
        /// A special result used as a padding value for legacy rulesets. It is a hit type and affects combo, but does not affect the base score (does not affect accuracy).
        /// </summary>
        /// <remarks>
        /// DO NOT USE.
        /// </remarks>
        [EnumMember(Value = "legacy_combo_increase")]
        [Order(99)]
        [Obsolete("Do not use.")]
        LegacyComboIncrease = 99
    }

#pragma warning disable CS0618
    public static class HitResultExtensions
    {
        private static readonly IList<HitResult> order = EnumExtensions.GetValuesInOrder<HitResult>().ToList();

        /// <summary>
        /// Whether a <see cref="HitResult"/> increases the combo.
        /// </summary>
        public static bool IncreasesCombo(this HitResult result)
            => AffectsCombo(result) && IsHit(result);

        /// <summary>
        /// Whether a <see cref="HitResult"/> breaks the combo and resets it back to zero.
        /// </summary>
        public static bool BreaksCombo(this HitResult result)
            => AffectsCombo(result) && !IsHit(result);

        /// <summary>
        /// Whether a <see cref="HitResult"/> increases or breaks the combo.
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
                case HitResult.LegacyComboIncrease:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Whether a <see cref="HitResult"/> affects the accuracy portion of the score.
        /// </summary>
        public static bool AffectsAccuracy(this HitResult result)
        {
            // LegacyComboIncrease is a special type which is neither a basic, tick, bonus, or accuracy-affecting result.
            if (result == HitResult.LegacyComboIncrease)
                return false;

            return IsScorable(result) && !IsBonus(result);
        }

        /// <summary>
        /// Whether a <see cref="HitResult"/> is a non-tick and non-bonus result.
        /// </summary>
        public static bool IsBasic(this HitResult result)
        {
            // LegacyComboIncrease is a special type which is neither a basic, tick, bonus, or accuracy-affecting result.
            if (result == HitResult.LegacyComboIncrease)
                return false;

            return IsScorable(result) && !IsTick(result) && !IsBonus(result);
        }

        /// <summary>
        /// Whether a <see cref="HitResult"/> should be counted as a tick.
        /// </summary>
        public static bool IsTick(this HitResult result)
        {
            switch (result)
            {
                case HitResult.LargeTickHit:
                case HitResult.LargeTickMiss:
                case HitResult.SmallTickHit:
                case HitResult.SmallTickMiss:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Whether a <see cref="HitResult"/> should be counted as bonus score.
        /// </summary>
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
                case HitResult.IgnoreMiss:
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
        public static bool IsScorable(this HitResult result)
        {
            // LegacyComboIncrease is not actually scorable (in terms of usable by rulesets for that purpose), but needs to be defined as such to be correctly included in statistics output.
            if (result == HitResult.LegacyComboIncrease)
                return true;

            return result >= HitResult.Miss && result < HitResult.IgnoreMiss;
        }

        /// <summary>
        /// An array of all scorable <see cref="HitResult"/>s.
        /// </summary>
        public static readonly HitResult[] ALL_TYPES = Enum.GetValues<HitResult>().Except(new[] { HitResult.LegacyComboIncrease }).ToArray();

        /// <summary>
        /// Whether a <see cref="HitResult"/> is valid within a given <see cref="HitResult"/> range.
        /// </summary>
        /// <param name="result">The <see cref="HitResult"/> to check.</param>
        /// <param name="minResult">The minimum <see cref="HitResult"/>.</param>
        /// <param name="maxResult">The maximum <see cref="HitResult"/>.</param>
        /// <returns>Whether <see cref="HitResult"/> falls between <paramref name="minResult"/> and <paramref name="maxResult"/>.</returns>
        public static bool IsValidHitResult(this HitResult result, HitResult minResult, HitResult maxResult)
        {
            if (result == HitResult.None)
                return false;

            if (result == minResult || result == maxResult)
                return true;

            Debug.Assert(minResult <= maxResult);
            return result > minResult && result < maxResult;
        }

        /// <summary>
        /// Ordered index of a <see cref="HitResult"/>. Used for consistent order when displaying hit results to the user.
        /// </summary>
        /// <param name="result">The <see cref="HitResult"/> to get the index of.</param>
        /// <returns>The index of <paramref name="result"/>.</returns>
        public static int GetIndexForOrderedDisplay(this HitResult result) => order.IndexOf(result);
    }
#pragma warning restore CS0618
}
