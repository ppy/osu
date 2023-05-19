// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Localisation;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Replays;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Scoring
{
    public partial class ScoreProcessor : JudgementProcessor
    {
        public const double MAX_SCORE = 1000000;

        private const double accuracy_cutoff_x = 1;
        private const double accuracy_cutoff_s = 0.95;
        private const double accuracy_cutoff_a = 0.9;
        private const double accuracy_cutoff_b = 0.8;
        private const double accuracy_cutoff_c = 0.7;
        private const double accuracy_cutoff_d = 0;

        /// <summary>
        /// Invoked when this <see cref="ScoreProcessor"/> was reset from a replay frame.
        /// </summary>
        public event Action? OnResetFromReplayFrame;

        /// <summary>
        /// The current total score.
        /// </summary>
        public readonly BindableLong TotalScore = new BindableLong { MinValue = 0 };

        /// <summary>
        /// The current accuracy.
        /// </summary>
        public readonly BindableDouble Accuracy = new BindableDouble(1) { MinValue = 0, MaxValue = 1 };

        /// <summary>
        /// The minimum achievable accuracy for the whole beatmap at this stage of gameplay.
        /// Assumes that all objects that have not been judged yet will receive the minimum hit result.
        /// </summary>
        public readonly BindableDouble MinimumAccuracy = new BindableDouble { MinValue = 0, MaxValue = 1 };

        /// <summary>
        /// The maximum achievable accuracy for the whole beatmap at this stage of gameplay.
        /// Assumes that all objects that have not been judged yet will receive the maximum hit result.
        /// </summary>
        public readonly BindableDouble MaximumAccuracy = new BindableDouble(1) { MinValue = 0, MaxValue = 1 };

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
        /// The <see cref="HitEvent"/>s collected during gameplay thus far.
        /// Intended for use with various statistics displays.
        /// </summary>
        public IReadOnlyList<HitEvent> HitEvents => hitEvents;

        /// <summary>
        /// The ruleset this score processor is valid for.
        /// </summary>
        public readonly Ruleset Ruleset;

        /// <summary>
        /// The maximum achievable total score.
        /// </summary>
        public long MaximumTotalScore { get; private set; }

        /// <summary>
        /// The maximum sum of accuracy-affecting judgements at the current point in time.
        /// </summary>
        /// <remarks>
        /// Used to compute accuracy.
        /// </remarks>
        private double currentMaximumBaseScore;

        /// <summary>
        /// The sum of all accuracy-affecting judgements at the current point in time.
        /// </summary>
        /// <remarks>
        /// Used to compute accuracy.
        /// </remarks>
        private double currentBaseScore;

        /// <summary>
        /// The count of all basic judgements in the beatmap.
        /// </summary>
        private int maximumCountBasicJudgements;

        /// <summary>
        /// The count of basic judgements at the current point in time.
        /// </summary>
        private int currentCountBasicJudgements;

        /// <summary>
        /// The maximum combo score in the beatmap.
        /// </summary>
        private double maximumComboPortion;

        /// <summary>
        /// The combo score at the current point in time.
        /// </summary>
        private double currentComboPortion;

        /// <summary>
        /// The bonus score at the current point in time.
        /// </summary>
        private double currentBonusPortion;

        /// <summary>
        /// The total score multiplier.
        /// </summary>
        private double scoreMultiplier = 1;

        public Dictionary<HitResult, int> MaximumStatistics
        {
            get
            {
                if (!beatmapApplied)
                    throw new InvalidOperationException($"Cannot access maximum statistics before calling {nameof(ApplyBeatmap)}.");

                return new Dictionary<HitResult, int>(maximumResultCounts);
            }
        }

        private bool beatmapApplied;

        private readonly Dictionary<HitResult, int> scoreResultCounts = new Dictionary<HitResult, int>();
        private readonly Dictionary<HitResult, int> maximumResultCounts = new Dictionary<HitResult, int>();

        private readonly List<HitEvent> hitEvents = new List<HitEvent>();
        private HitObject? lastHitObject;

        public ScoreProcessor(Ruleset ruleset)
        {
            Ruleset = ruleset;

            Combo.ValueChanged += combo => HighestCombo.Value = Math.Max(HighestCombo.Value, combo.NewValue);
            Accuracy.ValueChanged += accuracy =>
            {
                Rank.Value = RankFromAccuracy(accuracy.NewValue);
                foreach (var mod in Mods.Value.OfType<IApplicableToScoreProcessor>())
                    Rank.Value = mod.AdjustRank(Rank.Value, accuracy.NewValue);
            };

            Mods.ValueChanged += mods =>
            {
                scoreMultiplier = 1;

                foreach (var m in mods.NewValue)
                    scoreMultiplier *= m.ScoreMultiplier;

                updateScore();
            };
        }

        public override void ApplyBeatmap(IBeatmap beatmap)
        {
            base.ApplyBeatmap(beatmap);
            beatmapApplied = true;
        }

        protected sealed override void ApplyResultInternal(JudgementResult result)
        {
            result.ComboAtJudgement = Combo.Value;
            result.HighestComboAtJudgement = HighestCombo.Value;

            if (result.FailedAtJudgement)
                return;

            scoreResultCounts[result.Type] = scoreResultCounts.GetValueOrDefault(result.Type) + 1;

            if (!result.Type.IsScorable())
                return;

            if (result.Type.IncreasesCombo())
                Combo.Value++;
            else if (result.Type.BreaksCombo())
                Combo.Value = 0;

            result.ComboAfterJudgement = Combo.Value;

            if (result.Type.IsBasic())
                currentCountBasicJudgements++;

            if (result.Type.AffectsAccuracy())
            {
                currentMaximumBaseScore += Judgement.ToNumericResult(result.Judgement.MaxResult);
                currentBaseScore += Judgement.ToNumericResult(result.Type);
            }

            if (result.Type.IsBonus())
                currentBonusPortion += GetBonusScoreChange(result);

            if (result.Type.AffectsCombo())
                currentComboPortion += GetComboScoreChange(result);

            ApplyScoreChange(result);

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

            scoreResultCounts[result.Type] = scoreResultCounts.GetValueOrDefault(result.Type) - 1;

            if (!result.Type.IsScorable())
                return;

            if (result.Type.IsBasic())
                currentCountBasicJudgements--;

            if (result.Type.AffectsAccuracy())
            {
                currentMaximumBaseScore -= Judgement.ToNumericResult(result.Judgement.MaxResult);
                currentBaseScore -= Judgement.ToNumericResult(result.Type);
            }

            if (result.Type.IsBonus())
                currentBonusPortion -= GetBonusScoreChange(result);

            if (result.Type.AffectsCombo())
                currentComboPortion -= GetComboScoreChange(result);

            RemoveScoreChange(result);

            Debug.Assert(hitEvents.Count > 0);
            lastHitObject = hitEvents[^1].LastHitObject;
            hitEvents.RemoveAt(hitEvents.Count - 1);

            updateScore();
        }

        protected virtual double GetBonusScoreChange(JudgementResult result) => Judgement.ToNumericResult(result.Type);

        protected virtual double GetComboScoreChange(JudgementResult result) => Judgement.ToNumericResult(result.Type) * (1 + result.ComboAfterJudgement / 10d);

        protected virtual void ApplyScoreChange(JudgementResult result)
        {
        }

        protected virtual void RemoveScoreChange(JudgementResult result)
        {
        }

        private void updateScore()
        {
            Accuracy.Value = currentMaximumBaseScore > 0 ? currentBaseScore / currentMaximumBaseScore : 1;

            double comboRatio = maximumComboPortion > 0 ? currentComboPortion / maximumComboPortion : 1;
            double accuracyRatio = maximumCountBasicJudgements > 0 ? (double)currentCountBasicJudgements / maximumCountBasicJudgements : 1;

            TotalScore.Value = (long)Math.Round(ComputeTotalScore(comboRatio, accuracyRatio, currentBonusPortion) * scoreMultiplier);
        }

        protected virtual double ComputeTotalScore(double comboRatio, double accuracyRatio, double bonusPortion)
        {
            return
                (int)Math.Round
                ((
                    700000 * comboRatio +
                    300000 * Math.Pow(Accuracy.Value, 10) * accuracyRatio +
                    bonusPortion
                ) * scoreMultiplier);
        }

        /// <summary>
        /// Resets this ScoreProcessor to a default state.
        /// </summary>
        /// <param name="storeResults">Whether to store the current state of the <see cref="ScoreProcessor"/> for future use.</param>
        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);

            hitEvents.Clear();
            lastHitObject = null;

            if (storeResults)
            {
                maximumComboPortion = currentComboPortion;
                maximumCountBasicJudgements = currentCountBasicJudgements;

                maximumResultCounts.Clear();
                maximumResultCounts.AddRange(scoreResultCounts);

                MaximumTotalScore = TotalScore.Value;
            }

            scoreResultCounts.Clear();

            currentBaseScore = 0;
            currentMaximumBaseScore = 0;
            currentCountBasicJudgements = 0;
            currentComboPortion = 0;
            currentBonusPortion = 0;

            TotalScore.Value = 0;
            Accuracy.Value = 1;
            Combo.Value = 0;
            Rank.Disabled = false;
            Rank.Value = ScoreRank.X;
            HighestCombo.Value = 0;

            currentBaseScore = 0;
            currentMaximumBaseScore = 0;
        }

        /// <summary>
        /// Retrieve a score populated with data for the current play this processor is responsible for.
        /// </summary>
        public virtual void PopulateScore(ScoreInfo score)
        {
            score.Combo = Combo.Value;
            score.MaxCombo = HighestCombo.Value;
            score.Accuracy = Accuracy.Value;
            score.Rank = Rank.Value;
            score.HitEvents = hitEvents;
            score.Statistics.Clear();
            score.MaximumStatistics.Clear();

            foreach (var result in HitResultExtensions.ALL_TYPES)
                score.Statistics[result] = scoreResultCounts.GetValueOrDefault(result);

            foreach (var result in HitResultExtensions.ALL_TYPES)
                score.MaximumStatistics[result] = maximumResultCounts.GetValueOrDefault(result);

            // Populate total score after everything else.
            score.TotalScore = TotalScore.Value;
        }

        /// <summary>
        /// Populates a failed score, marking it with the <see cref="ScoreRank.F"/> rank.
        /// </summary>
        public void FailScore(ScoreInfo score)
        {
            if (Rank.Value == ScoreRank.F)
                return;

            score.Passed = false;
            Rank.Value = ScoreRank.F;

            PopulateScore(score);
        }

        public override void ResetFromReplayFrame(ReplayFrame frame)
        {
            base.ResetFromReplayFrame(frame);

            if (frame.Header == null)
                return;

            Combo.Value = frame.Header.Combo;
            HighestCombo.Value = frame.Header.MaxCombo;
            TotalScore.Value = frame.Header.TotalScore;

            scoreResultCounts.Clear();
            scoreResultCounts.AddRange(frame.Header.Statistics);

            ReadScoreProcessorStatistics(frame.Header.ScoreProcessorStatistics);

            updateScore();

            OnResetFromReplayFrame?.Invoke();
        }

        public virtual void WriteScoreProcessorStatistics(IDictionary<string, object> statistics)
        {
            statistics.Add(nameof(currentMaximumBaseScore), currentMaximumBaseScore);
            statistics.Add(nameof(currentBaseScore), currentBaseScore);
            statistics.Add(nameof(currentCountBasicJudgements), currentCountBasicJudgements);
            statistics.Add(nameof(currentComboPortion), currentComboPortion);
            statistics.Add(nameof(currentBonusPortion), currentBonusPortion);
        }

        public virtual void ReadScoreProcessorStatistics(IReadOnlyDictionary<string, object> statistics)
        {
            currentMaximumBaseScore = (double)statistics.GetValueOrDefault(nameof(currentMaximumBaseScore), 0);
            currentBaseScore = (double)statistics.GetValueOrDefault(nameof(currentBaseScore), 0);
            currentCountBasicJudgements = (int)statistics.GetValueOrDefault(nameof(currentCountBasicJudgements), 0);
            currentComboPortion = (double)statistics.GetValueOrDefault(nameof(currentComboPortion), 0);
            currentBonusPortion = (double)statistics.GetValueOrDefault(nameof(currentBonusPortion), 0);
        }

        #region Static helper methods

        /// <summary>
        /// Given an accuracy (0..1), return the correct <see cref="ScoreRank"/>.
        /// </summary>
        public static ScoreRank RankFromAccuracy(double accuracy)
        {
            if (accuracy == accuracy_cutoff_x)
                return ScoreRank.X;
            if (accuracy >= accuracy_cutoff_s)
                return ScoreRank.S;
            if (accuracy >= accuracy_cutoff_a)
                return ScoreRank.A;
            if (accuracy >= accuracy_cutoff_b)
                return ScoreRank.B;
            if (accuracy >= accuracy_cutoff_c)
                return ScoreRank.C;

            return ScoreRank.D;
        }

        /// <summary>
        /// Given a <see cref="ScoreRank"/>, return the cutoff accuracy (0..1).
        /// Accuracy must be greater than or equal to the cutoff to qualify for the provided rank.
        /// </summary>
        public static double AccuracyCutoffFromRank(ScoreRank rank)
        {
            switch (rank)
            {
                case ScoreRank.X:
                case ScoreRank.XH:
                    return accuracy_cutoff_x;

                case ScoreRank.S:
                case ScoreRank.SH:
                    return accuracy_cutoff_s;

                case ScoreRank.A:
                    return accuracy_cutoff_a;

                case ScoreRank.B:
                    return accuracy_cutoff_b;

                case ScoreRank.C:
                    return accuracy_cutoff_c;

                case ScoreRank.D:
                    return accuracy_cutoff_d;

                default:
                    throw new ArgumentOutOfRangeException(nameof(rank), rank, null);
            }
        }

        #endregion

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            hitEvents.Clear();
        }
    }

    public enum ScoringMode
    {
        [LocalisableDescription(typeof(GameplaySettingsStrings), nameof(GameplaySettingsStrings.StandardisedScoreDisplay))]
        Standardised,

        [LocalisableDescription(typeof(GameplaySettingsStrings), nameof(GameplaySettingsStrings.ClassicScoreDisplay))]
        Classic
    }
}
