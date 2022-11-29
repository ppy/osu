// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Replays;
using osu.Game.Scoring;
using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Rulesets.Scoring
{
    public partial class ScoreProcessor : JudgementProcessor
    {
        private const double max_score = 1000000;

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
        /// The <see cref="HitEvent"/>s collected during gameplay thus far.
        /// Intended for use with various statistics displays.
        /// </summary>
        public IReadOnlyList<HitEvent> HitEvents => hitEvents;

        /// <summary>
        /// The default portion of <see cref="max_score"/> awarded for hitting <see cref="HitObject"/>s accurately. Defaults to 30%.
        /// </summary>
        protected virtual double DefaultAccuracyPortion => 0.3;

        /// <summary>
        /// The default portion of <see cref="max_score"/> awarded for achieving a high combo. Default to 70%.
        /// </summary>
        protected virtual double DefaultComboPortion => 0.7;

        /// <summary>
        /// An arbitrary multiplier to scale scores in the <see cref="ScoringMode.Classic"/> scoring mode.
        /// </summary>
        protected virtual double ClassicScoreMultiplier => 36;

        private readonly Ruleset ruleset;
        private readonly double accuracyPortion;
        private readonly double comboPortion;

        /// <summary>
        /// Scoring values for a perfect play.
        /// </summary>
        public ScoringValues MaximumScoringValues
        {
            get
            {
                if (!beatmapApplied)
                    throw new InvalidOperationException($"Cannot access maximum scoring values before calling {nameof(ApplyBeatmap)}.");

                return maximumScoringValues;
            }
        }

        private ScoringValues maximumScoringValues;

        /// <summary>
        /// Scoring values for the current play assuming all perfect hits.
        /// </summary>
        /// <remarks>
        /// This is only used to determine the accuracy with respect to the current point in time for an ongoing play session.
        /// </remarks>
        private ScoringValues currentMaximumScoringValues;

        /// <summary>
        /// Scoring values for the current play.
        /// </summary>
        private ScoringValues currentScoringValues;

        /// <summary>
        /// The maximum <see cref="HitResult"/> of a basic (non-tick and non-bonus) hitobject.
        /// Only populated via <see cref="ComputeScore(osu.Game.Rulesets.Scoring.ScoringMode,osu.Game.Scoring.ScoreInfo)"/> or <see cref="ResetFromReplayFrame"/>.
        /// </summary>
        private HitResult? maxBasicResult;

        private bool beatmapApplied;

        private readonly Dictionary<HitResult, int> scoreResultCounts = new Dictionary<HitResult, int>();
        private readonly Dictionary<HitResult, int> maximumResultCounts = new Dictionary<HitResult, int>();

        private readonly List<HitEvent> hitEvents = new List<HitEvent>();
        private HitObject? lastHitObject;

        private double scoreMultiplier = 1;

        public ScoreProcessor(Ruleset ruleset)
        {
            this.ruleset = ruleset;

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

            // Always update the maximum scoring values.
            applyResult(result.Judgement.MaxResult, ref currentMaximumScoringValues);
            currentMaximumScoringValues.MaxCombo += result.Judgement.MaxResult.IncreasesCombo() ? 1 : 0;

            if (!result.Type.IsScorable())
                return;

            if (result.Type.IncreasesCombo())
                Combo.Value++;
            else if (result.Type.BreaksCombo())
                Combo.Value = 0;

            applyResult(result.Type, ref currentScoringValues);
            currentScoringValues.MaxCombo = HighestCombo.Value;

            hitEvents.Add(CreateHitEvent(result));
            lastHitObject = result.HitObject;

            updateScore();
        }

        private static void applyResult(HitResult result, ref ScoringValues scoringValues)
        {
            if (!result.IsScorable())
                return;

            if (result.IsBonus())
                scoringValues.BonusScore += result.IsHit() ? Judgement.ToNumericResult(result) : 0;
            else
                scoringValues.BaseScore += result.IsHit() ? Judgement.ToNumericResult(result) : 0;

            if (result.IsBasic())
                scoringValues.CountBasicHitObjects++;
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

            // Always update the maximum scoring values.
            revertResult(result.Judgement.MaxResult, ref currentMaximumScoringValues);
            currentMaximumScoringValues.MaxCombo -= result.Judgement.MaxResult.IncreasesCombo() ? 1 : 0;

            if (!result.Type.IsScorable())
                return;

            revertResult(result.Type, ref currentScoringValues);
            currentScoringValues.MaxCombo = HighestCombo.Value;

            Debug.Assert(hitEvents.Count > 0);
            lastHitObject = hitEvents[^1].LastHitObject;
            hitEvents.RemoveAt(hitEvents.Count - 1);

            updateScore();
        }

        private static void revertResult(HitResult result, ref ScoringValues scoringValues)
        {
            if (!result.IsScorable())
                return;

            if (result.IsBonus())
                scoringValues.BonusScore -= result.IsHit() ? Judgement.ToNumericResult(result) : 0;
            else
                scoringValues.BaseScore -= result.IsHit() ? Judgement.ToNumericResult(result) : 0;

            if (result.IsBasic())
                scoringValues.CountBasicHitObjects--;
        }

        private void updateScore()
        {
            Accuracy.Value = currentMaximumScoringValues.BaseScore > 0 ? (double)currentScoringValues.BaseScore / currentMaximumScoringValues.BaseScore : 1;
            TotalScore.Value = ComputeScore(Mode.Value, currentScoringValues, maximumScoringValues);
        }

        /// <summary>
        /// Computes the accuracy of a given <see cref="ScoreInfo"/>.
        /// </summary>
        /// <param name="scoreInfo">The <see cref="ScoreInfo"/> to compute the total score of.</param>
        /// <returns>The score's accuracy.</returns>
        [Pure]
        public double ComputeAccuracy(ScoreInfo scoreInfo)
        {
            if (!ruleset.RulesetInfo.Equals(scoreInfo.Ruleset))
                throw new ArgumentException($"Unexpected score ruleset. Expected \"{ruleset.RulesetInfo.ShortName}\" but was \"{scoreInfo.Ruleset.ShortName}\".");

            // We only extract scoring values from the score's statistics. This is because accuracy is always relative to the point of pass or fail rather than relative to the whole beatmap.
            extractScoringValues(scoreInfo.Statistics, out var current, out var maximum);

            return maximum.BaseScore > 0 ? current.BaseScore / maximum.BaseScore : 1;
        }

        /// <summary>
        /// Computes the total score of a given <see cref="ScoreInfo"/>.
        /// </summary>
        /// <remarks>
        /// Does not require <see cref="JudgementProcessor.ApplyBeatmap"/> to have been called before use.
        /// </remarks>
        /// <param name="mode">The <see cref="ScoringMode"/> to represent the score as.</param>
        /// <param name="scoreInfo">The <see cref="ScoreInfo"/> to compute the total score of.</param>
        /// <returns>The total score in the given <see cref="ScoringMode"/>.</returns>
        [Pure]
        public long ComputeScore(ScoringMode mode, ScoreInfo scoreInfo)
        {
            if (!ruleset.RulesetInfo.Equals(scoreInfo.Ruleset))
                throw new ArgumentException($"Unexpected score ruleset. Expected \"{ruleset.RulesetInfo.ShortName}\" but was \"{scoreInfo.Ruleset.ShortName}\".");

            ExtractScoringValues(scoreInfo, out var current, out var maximum);

            return ComputeScore(mode, current, maximum);
        }

        /// <summary>
        /// Computes the total score from scoring values.
        /// </summary>
        /// <param name="mode">The <see cref="ScoringMode"/> to represent the score as.</param>
        /// <param name="current">The current scoring values.</param>
        /// <param name="maximum">The maximum scoring values.</param>
        /// <returns>The total score computed from the given scoring values.</returns>
        [Pure]
        public long ComputeScore(ScoringMode mode, ScoringValues current, ScoringValues maximum)
        {
            double accuracyRatio = maximum.BaseScore > 0 ? (double)current.BaseScore / maximum.BaseScore : 1;
            double comboRatio = maximum.MaxCombo > 0 ? (double)current.MaxCombo / maximum.MaxCombo : 1;
            return ComputeScore(mode, accuracyRatio, comboRatio, current.BonusScore, maximum.CountBasicHitObjects);
        }

        /// <summary>
        /// Computes the total score from individual scoring components.
        /// </summary>
        /// <param name="mode">The <see cref="ScoringMode"/> to represent the score as.</param>
        /// <param name="accuracyRatio">The accuracy percentage achieved by the player.</param>
        /// <param name="comboRatio">The portion of the max combo achieved by the player.</param>
        /// <param name="bonusScore">The total bonus score.</param>
        /// <param name="totalBasicHitObjects">The total number of basic (non-tick and non-bonus) hitobjects in the beatmap.</param>
        /// <returns>The total score computed from the given scoring component ratios.</returns>
        [Pure]
        public long ComputeScore(ScoringMode mode, double accuracyRatio, double comboRatio, long bonusScore, int totalBasicHitObjects)
        {
            double accuracyScore = accuracyPortion * accuracyRatio;
            double comboScore = comboPortion * comboRatio;
            double rawScore = (max_score * (accuracyScore + comboScore) + bonusScore) * scoreMultiplier;

            switch (mode)
            {
                default:
                case ScoringMode.Standardised:
                    return (long)Math.Round(rawScore);

                case ScoringMode.Classic:
                    // This gives a similar feeling to osu!stable scoring (ScoreV1) while keeping classic scoring as only a constant multiple of standardised scoring.
                    // The invariant is important to ensure that scores don't get re-ordered on leaderboards between the two scoring modes.
                    double scaledRawScore = rawScore / max_score;
                    return (long)Math.Round(Math.Pow(scaledRawScore * Math.Max(1, totalBasicHitObjects), 2) * ClassicScoreMultiplier);
            }
        }

        private ScoreRank rankFrom(double acc)
        {
            if (acc == 1)
                return ScoreRank.X;
            if (acc >= 0.95)
                return ScoreRank.S;
            if (acc >= 0.9)
                return ScoreRank.A;
            if (acc >= 0.8)
                return ScoreRank.B;
            if (acc >= 0.7)
                return ScoreRank.C;

            return ScoreRank.D;
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
                maximumScoringValues = currentScoringValues;

                maximumResultCounts.Clear();
                maximumResultCounts.AddRange(scoreResultCounts);
            }

            scoreResultCounts.Clear();

            currentScoringValues = default;
            currentMaximumScoringValues = default;

            TotalScore.Value = 0;
            Accuracy.Value = 1;
            Combo.Value = 0;
            Rank.Disabled = false;
            Rank.Value = ScoreRank.X;
            HighestCombo.Value = 0;
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

            foreach (var result in HitResultExtensions.ALL_TYPES)
                score.Statistics[result] = scoreResultCounts.GetValueOrDefault(result);

            foreach (var result in HitResultExtensions.ALL_TYPES)
                score.MaximumStatistics[result] = maximumResultCounts.GetValueOrDefault(result);

            // Populate total score after everything else.
            score.TotalScore = ComputeScore(ScoringMode.Standardised, score);
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

            extractScoringValues(frame.Header.Statistics, out var current, out var maximum);
            currentScoringValues.BaseScore = current.BaseScore;
            currentScoringValues.MaxCombo = frame.Header.MaxCombo;
            currentMaximumScoringValues.BaseScore = maximum.BaseScore;
            currentMaximumScoringValues.MaxCombo = maximum.MaxCombo;

            Combo.Value = frame.Header.Combo;
            HighestCombo.Value = frame.Header.MaxCombo;

            scoreResultCounts.Clear();
            scoreResultCounts.AddRange(frame.Header.Statistics);

            updateScore();

            OnResetFromReplayFrame?.Invoke();
        }

        #region ScoringValue extraction

        /// <summary>
        /// Applies a best-effort extraction of hit statistics into <see cref="ScoringValues"/>.
        /// </summary>
        /// <remarks>
        /// This method is useful in a variety of situations, with a few drawbacks that need to be considered:
        /// <list type="bullet">
        ///     <item>The maximum <see cref="ScoringValues.BonusScore"/> will always be 0.</item>
        ///     <item>The current and maximum <see cref="ScoringValues.CountBasicHitObjects"/> will always be the same value.</item>
        /// </list>
        /// Consumers are expected to more accurately fill in the above values through external means.
        /// <para>
        /// <b>Ensure</b> to fill in the maximum <see cref="ScoringValues.CountBasicHitObjects"/> for use in
        /// <see cref="ComputeScore(osu.Game.Rulesets.Scoring.ScoringMode,osu.Game.Scoring.ScoringValues,osu.Game.Scoring.ScoringValues)"/>.
        /// </para>
        /// </remarks>
        /// <param name="scoreInfo">The score to extract scoring values from.</param>
        /// <param name="current">The "current" scoring values, representing the hit statistics as they appear.</param>
        /// <param name="maximum">The "maximum" scoring values, representing the hit statistics as if the maximum hit result was attained each time.</param>
        [Pure]
        internal void ExtractScoringValues(ScoreInfo scoreInfo, out ScoringValues current, out ScoringValues maximum)
        {
            extractScoringValues(scoreInfo.Statistics, out current, out maximum);
            current.MaxCombo = scoreInfo.MaxCombo;

            if (scoreInfo.MaximumStatistics.Count > 0)
                extractScoringValues(scoreInfo.MaximumStatistics, out _, out maximum);
        }

        /// <summary>
        /// Applies a best-effort extraction of hit statistics into <see cref="ScoringValues"/>.
        /// </summary>
        /// <remarks>
        /// This method is useful in a variety of situations, with a few drawbacks that need to be considered:
        /// <list type="bullet">
        ///     <item>The maximum <see cref="ScoringValues.BonusScore"/> will always be 0.</item>
        ///     <item>The current and maximum <see cref="ScoringValues.CountBasicHitObjects"/> will always be the same value.</item>
        /// </list>
        /// Consumers are expected to more accurately fill in the above values through external means.
        /// <para>
        /// <b>Ensure</b> to fill in the maximum <see cref="ScoringValues.CountBasicHitObjects"/> for use in
        /// <see cref="ComputeScore(osu.Game.Rulesets.Scoring.ScoringMode,osu.Game.Scoring.ScoringValues,osu.Game.Scoring.ScoringValues)"/>.
        /// </para>
        /// </remarks>
        /// <param name="header">The replay frame header to extract scoring values from.</param>
        /// <param name="current">The "current" scoring values, representing the hit statistics as they appear.</param>
        /// <param name="maximum">The "maximum" scoring values, representing the hit statistics as if the maximum hit result was attained each time.</param>
        [Pure]
        internal void ExtractScoringValues(FrameHeader header, out ScoringValues current, out ScoringValues maximum)
        {
            extractScoringValues(header.Statistics, out current, out maximum);
            current.MaxCombo = header.MaxCombo;
        }

        /// <summary>
        /// Applies a best-effort extraction of hit statistics into <see cref="ScoringValues"/>.
        /// </summary>
        /// <remarks>
        /// This method is useful in a variety of situations, with a few drawbacks that need to be considered:
        /// <list type="bullet">
        ///     <item>The current <see cref="ScoringValues.MaxCombo"/> will always be 0.</item>
        ///     <item>The maximum <see cref="ScoringValues.BonusScore"/> will always be 0.</item>
        ///     <item>The current and maximum <see cref="ScoringValues.CountBasicHitObjects"/> will always be the same value.</item>
        /// </list>
        /// Consumers are expected to more accurately fill in the above values (especially the current <see cref="ScoringValues.MaxCombo"/>) via external means (e.g. <see cref="ScoreInfo"/>).
        /// </remarks>
        /// <param name="statistics">The hit statistics to extract scoring values from.</param>
        /// <param name="current">The "current" scoring values, representing the hit statistics as they appear.</param>
        /// <param name="maximum">The "maximum" scoring values, representing the hit statistics as if the maximum hit result was attained each time.</param>
        [Pure]
        private void extractScoringValues(IReadOnlyDictionary<HitResult, int> statistics, out ScoringValues current, out ScoringValues maximum)
        {
            current = default;
            maximum = default;

            foreach ((HitResult result, int count) in statistics)
            {
                if (!result.IsScorable())
                    continue;

                if (result.IsBonus())
                    current.BonusScore += count * Judgement.ToNumericResult(result);

                if (result.AffectsAccuracy())
                {
                    // The maximum result of this judgement if it wasn't a miss.
                    // E.g. For a GOOD judgement, the max result is either GREAT/PERFECT depending on which one the ruleset uses (osu!: GREAT, osu!mania: PERFECT).
                    HitResult maxResult;

                    switch (result)
                    {
                        case HitResult.LargeTickHit:
                        case HitResult.LargeTickMiss:
                            maxResult = HitResult.LargeTickHit;
                            break;

                        case HitResult.SmallTickHit:
                        case HitResult.SmallTickMiss:
                            maxResult = HitResult.SmallTickHit;
                            break;

                        default:
                            maxResult = maxBasicResult ??= ruleset.GetHitResults().OrderByDescending(kvp => Judgement.ToNumericResult(kvp.result)).First().result;
                            break;
                    }

                    current.BaseScore += count * Judgement.ToNumericResult(result);
                    maximum.BaseScore += count * Judgement.ToNumericResult(maxResult);
                }

                if (result.AffectsCombo())
                    maximum.MaxCombo += count;

                if (result.IsBasic())
                {
                    current.CountBasicHitObjects += count;
                    maximum.CountBasicHitObjects += count;
                }
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
