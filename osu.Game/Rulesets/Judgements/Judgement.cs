// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Judgements
{
    /// <summary>
    /// The scoring information provided by a <see cref="HitObject"/>.
    /// </summary>
    public class Judgement
    {
        /// <summary>
        /// The default health increase for a maximum judgement, as a proportion of total health.
        /// By default, each maximum judgement restores 5% of total health.
        /// </summary>
        protected const double DEFAULT_MAX_HEALTH_INCREASE = 0.05;

        /// <summary>
        /// The maximum <see cref="HitResult"/> that can be achieved.
        /// </summary>
        public virtual HitResult MaxResult => HitResult.Perfect;

        /// <summary>
        /// The minimum <see cref="HitResult"/> that can be achieved - the inverse of <see cref="MaxResult"/>.
        /// </summary>
        /// <remarks>
        /// Defaults to a sane value for the given <see cref="MaxResult"/>. May be overridden to provide a supported custom value:
        /// <list type="table">
        ///   <listheader>
        ///     <term><see cref="MaxResult"/>s</term>
        ///     <description>Valid <see cref="MinResult"/>s</description>
        ///   </listheader>
        /// <item>
        ///   <term><see cref="HitResult.Perfect"/>, <see cref="HitResult.Great"/>, <see cref="HitResult.Good"/>, <see cref="HitResult.Ok"/>, <see cref="HitResult.Meh"/></term>
        ///   <description><see cref="HitResult.Miss"/></description>
        /// </item>
        /// <item>
        ///   <term><see cref="HitResult.LargeBonus"/></term>
        ///   <description><see cref="HitResult.IgnoreMiss"/></description>
        /// </item>
        /// <item>
        ///   <term><see cref="HitResult.SmallBonus"/></term>
        ///   <description><see cref="HitResult.IgnoreMiss"/></description>
        /// </item>
        /// <item>
        ///   <term><see cref="HitResult.SmallTickHit"/></term>
        ///   <description><see cref="HitResult.SmallTickMiss"/></description>
        /// </item>
        /// <item>
        ///   <term><see cref="HitResult.LargeTickHit"/></term>
        ///   <description><see cref="HitResult.LargeTickMiss"/></description>
        /// </item>
        /// <item>
        ///   <term><see cref="HitResult.IgnoreHit"/></term>
        ///   <description><see cref="HitResult.IgnoreMiss"/>, <see cref="HitResult.ComboBreak"/></description>
        /// </item>
        /// </list>
        /// </remarks>
        public virtual HitResult MinResult
        {
            get
            {
                switch (MaxResult)
                {
                    case HitResult.SmallBonus:
                    case HitResult.LargeBonus:
                    case HitResult.IgnoreHit:
                    case HitResult.HealthBonus:
                        return HitResult.IgnoreMiss;

                    case HitResult.SmallTickHit:
                        return HitResult.SmallTickMiss;

                    case HitResult.LargeTickHit:
                    case HitResult.SliderTailHit:
                        return HitResult.LargeTickMiss;

                    default:
                        return HitResult.Miss;
                }
            }
        }

        /// <summary>
        /// The health increase for the maximum achievable result.
        /// </summary>
        public double MaxHealthIncrease => HealthIncreaseFor(MaxResult);

        /// <summary>
        /// Retrieves the numeric health increase of a <see cref="HitResult"/>.
        /// </summary>
        /// <param name="result">The <see cref="HitResult"/> to find the numeric health increase for.</param>
        /// <returns>The numeric health increase of <paramref name="result"/>.</returns>
        protected virtual double HealthIncreaseFor(HitResult result)
        {
            switch (result)
            {
                default:
                    return 0;

                case HitResult.SmallTickHit:
                    return DEFAULT_MAX_HEALTH_INCREASE * 0.5;

                case HitResult.SmallTickMiss:
                    return -DEFAULT_MAX_HEALTH_INCREASE * 0.5;

                case HitResult.SliderTailHit:
                case HitResult.LargeTickHit:
                    return DEFAULT_MAX_HEALTH_INCREASE;

                case HitResult.LargeTickMiss:
                    return -DEFAULT_MAX_HEALTH_INCREASE;

                case HitResult.Miss:
                    return -DEFAULT_MAX_HEALTH_INCREASE * 2;

                case HitResult.Meh:
                    return DEFAULT_MAX_HEALTH_INCREASE * 0.05;

                case HitResult.Ok:
                    return DEFAULT_MAX_HEALTH_INCREASE * 0.5;

                case HitResult.Good:
                    return DEFAULT_MAX_HEALTH_INCREASE * 0.75;

                case HitResult.Great:
                    return DEFAULT_MAX_HEALTH_INCREASE;

                case HitResult.Perfect:
                    return DEFAULT_MAX_HEALTH_INCREASE * 1.05;

                case HitResult.SmallBonus:
                    return DEFAULT_MAX_HEALTH_INCREASE * 0.5;

                case HitResult.LargeBonus:
                    return DEFAULT_MAX_HEALTH_INCREASE;
            }
        }

        /// <summary>
        /// Retrieves the numeric health increase of a <see cref="JudgementResult"/>.
        /// </summary>
        /// <param name="result">The <see cref="JudgementResult"/> to find the numeric health increase for.</param>
        /// <returns>The numeric health increase of <paramref name="result"/>.</returns>
        public double HealthIncreaseFor(JudgementResult result) => HealthIncreaseFor(result.Type);

        public override string ToString() => $"MaxResult:{MaxResult}";
    }
}
