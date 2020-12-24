// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Utils;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Scoring
{
    public class ScoreProcessor : JudgementProcessor
    {
        private const double max_score = 1000000;

        /// <summary>
        /// The current total score.
        /// </summary>
        public readonly BindableDouble TotalScore = new BindableDouble { MinValue = 0 };

        /// <summary>
        /// The current accuracy.
        /// </summary>
        public readonly BindableDouble Accuracy = new BindableDouble(1) { MinValue = 0, MaxValue = 1 };

        /// <summary>
        /// The current combo.
        /// </summary>
        public readonly BindableInt Combo = new BindableInt();

        /// <summary>
        /// The current selected mods
        /// </summary>
        public readonly Bindable<IReadOnlyList<Mod>> Mods = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        /// <summary>
        /// The current rank.
        /// </summary>
        public readonly Bindable<ScoreRank> Rank = new Bindable<ScoreRank>(ScoreRank.X);

        /// <summary>
        /// The highest combo achieved by this score.
        /// </summary>
        public readonly BindableInt HighestCombo = new BindableInt();

        /// <summary>
        /// The <see cref="ScoringMode"/> used to calculate scores.
        /// </summary>
        public readonly Bindable<ScoringMode> Mode = new Bindable<ScoringMode>();

        /// <summary>
        /// The default portion of <see cref="max_score"/> awarded for hitting <see cref="HitObject"/>s accurately. Defaults to 30%.
        /// </summary>
        protected virtual double DefaultAccuracyPortion => 0.3;

        /// <summary>
        /// The default portion of <see cref="max_score"/> awarded for achieving a high combo. Default to 70%.
        /// </summary>
        protected virtual double DefaultComboPortion => 0.7;

        private readonly double accuracyPortion;
        private readonly double comboPortion;

        private int maxAchievableCombo;

        /// <summary>
        /// The maximum achievable base score.
        /// </summary>
        private double maxBaseScore;

        private double rollingMaxBaseScore;
        private double baseScore;

        private readonly List<HitEvent> hitEvents = new List<HitEvent>();
        private HitObject lastHitObject;

        private double scoreMultiplier = 1;

        public ScoreProcessor()
        {
            accuracyPortion = DefaultAccuracyPortion;
            comboPortion = DefaultComboPortion;

            if (!Precision.AlmostEquals(1.0, accuracyPortion + comboPortion))
                throw new InvalidOperationException($"{nameof(DefaultAccuracyPortion)} + {nameof(DefaultComboPortion)} must equal 1.");

            Combo.ValueChanged += combo => HighestCombo.Value = Math.Max(HighestCombo.Value, combo.NewValue);
            Accuracy.ValueChanged += accuracy =>
            {
                Rank.Value = rankFrom(accuracy.NewValue);
                foreach (var mod in Mods.Value.OfType<IApplicableToScoreProcessor>())
                    Rank.Value = mod.AdjustRank(Rank.Value, accuracy.NewValue);
            };

            Mode.ValueChanged += _ => updateScore();
            Mods.ValueChanged += mods =>
            {
                scoreMultiplier = 1;

                foreach (var m in mods.NewValue)
                    scoreMultiplier *= m.ScoreMultiplier;

                updateScore();
            };
        }

        private readonly Dictionary<HitResult, int> scoreResultCounts = new Dictionary<HitResult, int>();

        protected sealed override void ApplyResultInternal(JudgementResult result)
        {
            result.ComboAtJudgement = Combo.Value;
            result.HighestComboAtJudgement = HighestCombo.Value;

            if (result.FailedAtJudgement)
                return;

            if (!result.Type.IsScorable())
                return;

            if (result.Type.AffectsCombo())
            {
                switch (result.Type)
                {
                    case HitResult.Miss:
                    case HitResult.LargeTickMiss:
                        Combo.Value = 0;
                        break;

                    default:
                        Combo.Value++;
                        break;
                }
            }

            double scoreIncrease = result.Type.IsHit() ? result.Judgement.NumericResultFor(result) : 0;

            if (!result.Type.IsBonus())
            {
                baseScore += scoreIncrease;
                rollingMaxBaseScore += result.Judgement.MaxNumericResult;
            }

            scoreResultCounts[result.Type] = scoreResultCounts.GetOrDefault(result.Type) + 1;

            hitEvents.Add(CreateHitEvent(result));
            lastHitObject = result.HitObject;

            updateScore();
        }

        /// <summary>
        /// Creates the <see cref="HitEvent"/> that describes a <see cref="JudgementResult"/>.
        /// </summary>
        /// <param name="result">The <see cref="JudgementResult"/> to describe.</param>
        /// <returns>The <see cref="HitEvent"/>.</returns>
        protected virtual HitEvent CreateHitEvent(JudgementResult result)
            => new HitEvent(result.TimeOffset, result.Type, result.HitObject, lastHitObject, null);

        protected sealed override void RevertResultInternal(JudgementResult result)
        {
            Combo.Value = result.ComboAtJudgement;
            HighestCombo.Value = result.HighestComboAtJudgement;

            if (result.FailedAtJudgement)
                return;

            if (!result.Type.IsScorable())
                return;

            double scoreIncrease = result.Type.IsHit() ? result.Judgement.NumericResultFor(result) : 0;

            if (!result.Type.IsBonus())
            {
                baseScore -= scoreIncrease;
                rollingMaxBaseScore -= result.Judgement.MaxNumericResult;
            }

            scoreResultCounts[result.Type] = scoreResultCounts.GetOrDefault(result.Type) - 1;

            Debug.Assert(hitEvents.Count > 0);
            lastHitObject = hitEvents[^1].LastHitObject;
            hitEvents.RemoveAt(hitEvents.Count - 1);

            updateScore();
        }

        private void updateScore()
        {
            if (rollingMaxBaseScore != 0)
                Accuracy.Value = calculateAccuracyRatio(baseScore, true);

            TotalScore.Value = getScore(Mode.Value);
        }

        private double getScore(ScoringMode mode)
        {
            return GetScore(mode, maxAchievableCombo,
                calculateAccuracyRatio(baseScore),
                calculateComboRatio(HighestCombo.Value),
                scoreResultCounts);
        }

        /// <summary>
        /// Computes the total score.
        /// </summary>
        /// <param name="mode">The <see cref="ScoringMode"/> to compute the total score in.</param>
        /// <param name="maxCombo">The maximum combo achievable in the beatmap.</param>
        /// <param name="accuracyRatio">The accuracy percentage achieved by the player.</param>
        /// <param name="comboRatio">The proportion of <paramref name="maxCombo"/> achieved by the player.</param>
        /// <param name="statistics">Any statistics to be factored in.</param>
        /// <returns>The total score.</returns>
        public double GetScore(ScoringMode mode, int maxCombo, double accuracyRatio, double comboRatio, Dictionary<HitResult, int> statistics)
        {
            switch (mode)
            {
                default:
                case ScoringMode.Standardised:
                    double accuracyScore = accuracyPortion * accuracyRatio;
                    double comboScore = comboPortion * comboRatio;

                    return (max_score * (accuracyScore + comboScore) + getBonusScore(statistics)) * scoreMultiplier;

                case ScoringMode.Classic:
                    // should emulate osu-stable's scoring as closely as we can (https://osu.ppy.sh/help/wiki/Score/ScoreV1)
                    return getBonusScore(statistics) + (accuracyRatio * Math.Max(1, maxCombo) * 300) * (1 + Math.Max(0, (comboRatio * maxCombo) - 1) * scoreMultiplier / 25);
            }
        }

        /// <summary>
        /// Given a minimal set of inputs, return the computed score for the tracked beatmap / mods combination, at the current point in time.
        /// </summary>
        /// <param name="mode">The <see cref="ScoringMode"/> to compute the total score in.</param>
        /// <param name="maxCombo">The maximum combo achievable in the beatmap.</param>
        /// <param name="statistics">Statistics to be used for calculating accuracy, bonus score, etc.</param>
        /// <returns>The computed score for provided inputs.</returns>
        public double GetImmediateScore(ScoringMode mode, int maxCombo, Dictionary<HitResult, int> statistics)
        {
            // calculate base score from statistics pairs
            int computedBaseScore = 0;

            foreach (var pair in statistics)
            {
                if (!pair.Key.AffectsAccuracy())
                    continue;

                computedBaseScore += Judgement.ToNumericResult(pair.Key) * pair.Value;
            }

            return GetScore(mode, maxAchievableCombo, calculateAccuracyRatio(computedBaseScore), calculateComboRatio(maxCombo), scoreResultCounts);
        }

        /// <summary>
        /// Get the accuracy fraction for the provided base score.
        /// </summary>
        /// <param name="baseScore">The score to be used for accuracy calculation.</param>
        /// <param name="preferRolling">Whether the rolling base score should be used (ie. for the current point in time based on Apply/Reverted results).</param>
        /// <returns>The computed accuracy.</returns>
        private double calculateAccuracyRatio(double baseScore, bool preferRolling = false)
        {
            if (preferRolling && rollingMaxBaseScore != 0)
                return baseScore / rollingMaxBaseScore;

            return maxBaseScore > 0 ? baseScore / maxBaseScore : 0;
        }

        private double calculateComboRatio(int maxCombo) => maxAchievableCombo > 0 ? (double)maxCombo / maxAchievableCombo : 1;

        private double getBonusScore(Dictionary<HitResult, int> statistics)
            => statistics.GetOrDefault(HitResult.SmallBonus) * Judgement.SMALL_BONUS_SCORE
               + statistics.GetOrDefault(HitResult.LargeBonus) * Judgement.LARGE_BONUS_SCORE;

        private ScoreRank rankFrom(double acc)
        {
            if (acc == 1)
                return ScoreRank.X;
            if (acc > 0.95)
                return ScoreRank.S;
            if (acc > 0.9)
                return ScoreRank.A;
            if (acc > 0.8)
                return ScoreRank.B;
            if (acc > 0.7)
                return ScoreRank.C;

            return ScoreRank.D;
        }

        public int GetStatistic(HitResult result) => scoreResultCounts.GetOrDefault(result);

        public double GetStandardisedScore() => getScore(ScoringMode.Standardised);

        /// <summary>
        /// Resets this ScoreProcessor to a default state.
        /// </summary>
        /// <param name="storeResults">Whether to store the current state of the <see cref="ScoreProcessor"/> for future use.</param>
        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);

            scoreResultCounts.Clear();
            hitEvents.Clear();
            lastHitObject = null;

            if (storeResults)
            {
                maxAchievableCombo = HighestCombo.Value;
                maxBaseScore = baseScore;
            }

            baseScore = 0;
            rollingMaxBaseScore = 0;

            TotalScore.Value = 0;
            Accuracy.Value = 1;
            Combo.Value = 0;
            Rank.Value = ScoreRank.X;
            HighestCombo.Value = 0;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            hitEvents.Clear();
        }

        /// <summary>
        /// Retrieve a score populated with data for the current play this processor is responsible for.
        /// </summary>
        public virtual void PopulateScore(ScoreInfo score)
        {
            score.TotalScore = (long)Math.Round(GetStandardisedScore());
            score.Combo = Combo.Value;
            score.MaxCombo = HighestCombo.Value;
            score.Accuracy = Math.Round(Accuracy.Value, 4);
            score.Rank = Rank.Value;
            score.Date = DateTimeOffset.Now;

            foreach (var result in Enum.GetValues(typeof(HitResult)).OfType<HitResult>().Where(r => r.IsScorable()))
                score.Statistics[result] = GetStatistic(result);

            score.HitEvents = hitEvents;
        }

        /// <summary>
        /// Create a <see cref="HitWindows"/> for this processor.
        /// </summary>
        [Obsolete("Method is now unused.")] // Can be removed 20210328
        public virtual HitWindows CreateHitWindows() => new HitWindows();
    }

    public enum ScoringMode
    {
        Standardised,
        Classic
    }
}
