// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Scoring
{
    public class ScoreProcessor : JudgementProcessor
    {
        private const double base_portion = 0.3;
        private const double combo_portion = 0.7;
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

        private double maxHighestCombo;

        private double maxBaseScore;
        private double rollingMaxBaseScore;
        private double baseScore;
        private double bonusScore;

        private readonly List<HitEvent> hitEvents = new List<HitEvent>();
        private HitObject lastHitObject;

        private double scoreMultiplier = 1;

        public ScoreProcessor()
        {
            Debug.Assert(base_portion + combo_portion == 1.0);

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

            if (result.Judgement.AffectsCombo)
            {
                switch (result.Type)
                {
                    case HitResult.None:
                        break;

                    case HitResult.Miss:
                        Combo.Value = 0;
                        break;

                    default:
                        Combo.Value++;
                        break;
                }
            }

            if (result.Judgement.IsBonus)
            {
                if (result.IsHit)
                    bonusScore += result.Judgement.NumericResultFor(result);
            }
            else
            {
                if (result.HasResult)
                    scoreResultCounts[result.Type] = scoreResultCounts.GetOrDefault(result.Type) + 1;

                baseScore += result.Judgement.NumericResultFor(result);
                rollingMaxBaseScore += result.Judgement.MaxNumericResult;
            }

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

            if (result.Judgement.IsBonus)
            {
                if (result.IsHit)
                    bonusScore -= result.Judgement.NumericResultFor(result);
            }
            else
            {
                if (result.HasResult)
                    scoreResultCounts[result.Type] = scoreResultCounts.GetOrDefault(result.Type) - 1;

                baseScore -= result.Judgement.NumericResultFor(result);
                rollingMaxBaseScore -= result.Judgement.MaxNumericResult;
            }

            Debug.Assert(hitEvents.Count > 0);
            lastHitObject = hitEvents[^1].LastHitObject;
            hitEvents.RemoveAt(hitEvents.Count - 1);

            updateScore();
        }

        private void updateScore()
        {
            if (rollingMaxBaseScore != 0)
                Accuracy.Value = baseScore / rollingMaxBaseScore;

            TotalScore.Value = getScore(Mode.Value);
        }

        private double getScore(ScoringMode mode)
        {
            switch (mode)
            {
                default:
                case ScoringMode.Standardised:
                    return (max_score * (base_portion * baseScore / maxBaseScore + combo_portion * HighestCombo.Value / maxHighestCombo) + bonusScore) * scoreMultiplier;

                case ScoringMode.Classic:
                    // should emulate osu-stable's scoring as closely as we can (https://osu.ppy.sh/help/wiki/Score/ScoreV1)
                    return bonusScore + baseScore * (1 + Math.Max(0, HighestCombo.Value - 1) * scoreMultiplier / 25);
            }
        }

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
                maxHighestCombo = HighestCombo.Value;
                maxBaseScore = baseScore;

                if (maxBaseScore == 0 || maxHighestCombo == 0)
                {
                    Mode.Value = ScoringMode.Classic;
                    Mode.Disabled = true;
                }
            }

            baseScore = 0;
            rollingMaxBaseScore = 0;
            bonusScore = 0;

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
            score.TotalScore = (long)Math.Round(TotalScore.Value);
            score.Combo = Combo.Value;
            score.MaxCombo = HighestCombo.Value;
            score.Accuracy = Math.Round(Accuracy.Value, 4);
            score.Rank = Rank.Value;
            score.Date = DateTimeOffset.Now;

            var hitWindows = CreateHitWindows();

            foreach (var result in Enum.GetValues(typeof(HitResult)).OfType<HitResult>().Where(r => r > HitResult.None && hitWindows.IsHitResultAllowed(r)))
                score.Statistics[result] = GetStatistic(result);

            score.HitEvents = hitEvents;
        }

        /// <summary>
        /// Create a <see cref="HitWindows"/> for this processor.
        /// </summary>
        public virtual HitWindows CreateHitWindows() => new HitWindows();
    }

    public enum ScoringMode
    {
        Standardised,
        Classic
    }
}
