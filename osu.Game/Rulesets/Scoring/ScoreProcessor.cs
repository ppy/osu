// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Scoring
{
    public abstract class ScoreProcessor
    {
        /// <summary>
        /// Invoked when the <see cref="ScoreProcessor"/> is in a failed state.
        /// This may occur regardless of whether an <see cref="AllJudged"/> event is invoked.
        /// Return true if the fail was permitted.
        /// </summary>
        public event Func<bool> Failed;

        /// <summary>
        /// Invoked when all <see cref="HitObject"/>s have been judged.
        /// </summary>
        public event Action AllJudged;

        /// <summary>
        /// Invoked when a new judgement has occurred. This occurs after the judgement has been processed by the <see cref="ScoreProcessor"/>.
        /// </summary>
        public event Action<JudgementResult> NewJudgement;

        /// <summary>
        /// Additional conditions on top of <see cref="DefaultFailCondition"/> that cause a failing state.
        /// </summary>
        public event Func<ScoreProcessor, JudgementResult, bool> FailConditions;

        /// <summary>
        /// The current total score.
        /// </summary>
        public readonly BindableDouble TotalScore = new BindableDouble { MinValue = 0 };

        /// <summary>
        /// The current accuracy.
        /// </summary>
        public readonly BindableDouble Accuracy = new BindableDouble(1) { MinValue = 0, MaxValue = 1 };

        /// <summary>
        /// The current health.
        /// </summary>
        public readonly BindableDouble Health = new BindableDouble(1) { MinValue = 0, MaxValue = 1 };

        /// <summary>
        /// The current combo.
        /// </summary>
        public readonly BindableInt Combo = new BindableInt();

        /// <summary>
        /// The current selected mods
        /// </summary>
        public readonly Bindable<IReadOnlyList<Mod>> Mods = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        /// <summary>
        /// Create a <see cref="HitWindows"/> for this processor.
        /// </summary>
        public virtual HitWindows CreateHitWindows() => new HitWindows();

        /// <summary>
        /// The current rank.
        /// </summary>
        public readonly Bindable<ScoreRank> Rank = new Bindable<ScoreRank>(ScoreRank.X);

        /// <summary>
        /// THe highest combo achieved by this score.
        /// </summary>
        public readonly BindableInt HighestCombo = new BindableInt();

        /// <summary>
        /// The <see cref="ScoringMode"/> used to calculate scores.
        /// </summary>
        public readonly Bindable<ScoringMode> Mode = new Bindable<ScoringMode>();

        /// <summary>
        /// Whether all <see cref="Judgement"/>s have been processed.
        /// </summary>
        public virtual bool HasCompleted => false;

        /// <summary>
        /// The total number of judged <see cref="HitObject"/>s at the current point in time.
        /// </summary>
        public int JudgedHits { get; protected set; }

        /// <summary>
        /// Whether this ScoreProcessor has already triggered the failed state.
        /// </summary>
        public virtual bool HasFailed { get; private set; }

        /// <summary>
        /// The default conditions for failing.
        /// </summary>
        protected virtual bool DefaultFailCondition => Precision.AlmostBigger(Health.MinValue, Health.Value);

        protected ScoreProcessor()
        {
            Combo.ValueChanged += delegate { HighestCombo.Value = Math.Max(HighestCombo.Value, Combo.Value); };
            Accuracy.ValueChanged += delegate
            {
                Rank.Value = rankFrom(Accuracy.Value);
                foreach (var mod in Mods.Value.OfType<IApplicableToScoreProcessor>())
                    Rank.Value = mod.AdjustRank(Rank.Value, Accuracy.Value);
            };
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

        /// <summary>
        /// Resets this ScoreProcessor to a default state.
        /// </summary>
        /// <param name="storeResults">Whether to store the current state of the <see cref="ScoreProcessor"/> for future use.</param>
        protected virtual void Reset(bool storeResults)
        {
            TotalScore.Value = 0;
            Accuracy.Value = 1;
            Health.Value = 1;
            Combo.Value = 0;
            Rank.Value = ScoreRank.X;
            HighestCombo.Value = 0;

            JudgedHits = 0;

            HasFailed = false;
        }

        /// <summary>
        /// Checks if the score is in a failed state and notifies subscribers.
        /// <para>
        /// This can only ever notify subscribers once.
        /// </para>
        /// </summary>
        protected void UpdateFailed(JudgementResult result)
        {
            if (HasFailed)
                return;

            if (!DefaultFailCondition && FailConditions?.Invoke(this, result) != true)
                return;

            if (Failed?.Invoke() != false)
                HasFailed = true;
        }

        /// <summary>
        /// Notifies subscribers of <see cref="NewJudgement"/> that a new judgement has occurred.
        /// </summary>
        /// <param name="result">The judgement scoring result to notify subscribers of.</param>
        protected void NotifyNewJudgement(JudgementResult result)
        {
            NewJudgement?.Invoke(result);

            if (HasCompleted)
                AllJudged?.Invoke();
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
        }

        public abstract int GetStatistic(HitResult result);

        public abstract double GetStandardisedScore();
    }

    public class ScoreProcessor<TObject> : ScoreProcessor
        where TObject : HitObject
    {
        private const double base_portion = 0.3;
        private const double combo_portion = 0.7;
        private const double max_score = 1000000;

        public sealed override bool HasCompleted => JudgedHits == MaxHits;

        protected int MaxHits { get; private set; }

        private double maxHighestCombo;

        private double maxBaseScore;
        private double rollingMaxBaseScore;
        private double baseScore;
        private double bonusScore;

        private double scoreMultiplier = 1;

        public ScoreProcessor(DrawableRuleset<TObject> drawableRuleset)
        {
            Debug.Assert(base_portion + combo_portion == 1.0);

            drawableRuleset.OnNewResult += applyResult;
            drawableRuleset.OnRevertResult += revertResult;

            ApplyBeatmap(drawableRuleset.Beatmap);

            Reset(false);
            SimulateAutoplay(drawableRuleset.Beatmap);
            Reset(true);

            if (maxBaseScore == 0 || maxHighestCombo == 0)
            {
                Mode.Value = ScoringMode.Classic;
                Mode.Disabled = true;
            }

            Mode.ValueChanged += _ => updateScore();
            Mods.ValueChanged += mods =>
            {
                scoreMultiplier = 1;

                foreach (var m in mods.NewValue)
                    scoreMultiplier *= m.ScoreMultiplier;

                updateScore();
            };
        }

        /// <summary>
        /// Applies any properties of the <see cref="Beatmap{TObject}"/> which affect scoring to this <see cref="ScoreProcessor{TObject}"/>.
        /// </summary>
        /// <param name="beatmap">The <see cref="Beatmap{TObject}"/> to read properties from.</param>
        protected virtual void ApplyBeatmap(Beatmap<TObject> beatmap)
        {
        }

        /// <summary>
        /// Simulates an autoplay of the <see cref="Beatmap{TObject}"/> to determine scoring values.
        /// </summary>
        /// <remarks>This provided temporarily. DO NOT USE.</remarks>
        /// <param name="beatmap">The <see cref="Beatmap{TObject}"/> to simulate.</param>
        protected virtual void SimulateAutoplay(Beatmap<TObject> beatmap)
        {
            foreach (var obj in beatmap.HitObjects)
                simulate(obj);

            void simulate(HitObject obj)
            {
                foreach (var nested in obj.NestedHitObjects)
                    simulate(nested);

                var judgement = obj.CreateJudgement();
                if (judgement == null)
                    return;

                var result = CreateResult(obj, judgement);
                if (result == null)
                    throw new InvalidOperationException($"{GetType().ReadableName()} must provide a {nameof(JudgementResult)} through {nameof(CreateResult)}.");

                result.Type = judgement.MaxResult;

                applyResult(result);
            }
        }

        /// <summary>
        /// Applies the score change of a <see cref="JudgementResult"/> to this <see cref="ScoreProcessor"/>.
        /// </summary>
        /// <param name="result">The <see cref="JudgementResult"/> to apply.</param>
        private void applyResult(JudgementResult result)
        {
            ApplyResult(result);
            updateScore();

            UpdateFailed(result);
            NotifyNewJudgement(result);
        }

        /// <summary>
        /// Reverts the score change of a <see cref="JudgementResult"/> that was applied to this <see cref="ScoreProcessor"/>.
        /// </summary>
        /// <param name="result">The judgement scoring result.</param>
        private void revertResult(JudgementResult result)
        {
            RevertResult(result);
            updateScore();
        }

        private readonly Dictionary<HitResult, int> scoreResultCounts = new Dictionary<HitResult, int>();

        /// <summary>
        /// Applies the score change of a <see cref="JudgementResult"/> to this <see cref="ScoreProcessor"/>.
        /// </summary>
        /// <remarks>
        /// Any changes applied via this method can be reverted via <see cref="RevertResult"/>.
        /// </remarks>
        /// <param name="result">The <see cref="JudgementResult"/> to apply.</param>
        protected virtual void ApplyResult(JudgementResult result)
        {
            result.ComboAtJudgement = Combo.Value;
            result.HighestComboAtJudgement = HighestCombo.Value;
            result.HealthAtJudgement = Health.Value;
            result.FailedAtJudgement = HasFailed;

            if (HasFailed)
                return;

            JudgedHits++;

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

            Health.Value += HealthAdjustmentFactorFor(result) * result.Judgement.HealthIncreaseFor(result);
        }

        /// <summary>
        /// Reverts the score change of a <see cref="JudgementResult"/> that was applied to this <see cref="ScoreProcessor"/> via <see cref="ApplyResult"/>.
        /// </summary>
        /// <param name="result">The judgement scoring result.</param>
        protected virtual void RevertResult(JudgementResult result)
        {
            Combo.Value = result.ComboAtJudgement;
            HighestCombo.Value = result.HighestComboAtJudgement;
            Health.Value = result.HealthAtJudgement;

            // Todo: Revert HasFailed state with proper player support

            if (result.FailedAtJudgement)
                return;

            JudgedHits--;

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
        }

        /// <summary>
        /// An adjustment factor which is multiplied into the health increase provided by a <see cref="JudgementResult"/>.
        /// </summary>
        /// <param name="result">The <see cref="JudgementResult"/> for which the adjustment should apply.</param>
        /// <returns>The adjustment factor.</returns>
        protected virtual double HealthAdjustmentFactorFor(JudgementResult result) => 1;

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
                    return bonusScore + baseScore * ((1 + Math.Max(0, HighestCombo.Value - 1) * scoreMultiplier) / 25);
            }
        }

        public override int GetStatistic(HitResult result) => scoreResultCounts.GetOrDefault(result);

        public override double GetStandardisedScore() => getScore(ScoringMode.Standardised);

        protected override void Reset(bool storeResults)
        {
            scoreResultCounts.Clear();

            if (storeResults)
            {
                MaxHits = JudgedHits;
                maxHighestCombo = HighestCombo.Value;
                maxBaseScore = baseScore;
            }

            base.Reset(storeResults);

            baseScore = 0;
            rollingMaxBaseScore = 0;
            bonusScore = 0;
        }

        /// <summary>
        /// Creates the <see cref="JudgementResult"/> that represents the scoring result for a <see cref="HitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> which was judged.</param>
        /// <param name="judgement">The <see cref="Judgement"/> that provides the scoring information.</param>
        protected virtual JudgementResult CreateResult(HitObject hitObject, Judgement judgement) => new JudgementResult(hitObject, judgement);
    }

    public enum ScoringMode
    {
        Standardised,
        Classic
    }
}
