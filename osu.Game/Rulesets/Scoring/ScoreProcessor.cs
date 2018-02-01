// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Diagnostics;
using osu.Framework.Configuration;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;

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
        public event Action<Judgement> NewJudgement;

        /// <summary>
        /// Additional conditions on top of <see cref="DefaultFailCondition"/> that cause a failing state.
        /// </summary>
        public event Func<ScoreProcessor, bool> FailConditions;

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
        public readonly BindableDouble Health = new BindableDouble { MinValue = 0, MaxValue = 1 };

        /// <summary>
        /// The current combo.
        /// </summary>
        public readonly BindableInt Combo = new BindableInt();

        /// <summary>
        /// The current rank.
        /// </summary>
        public readonly Bindable<ScoreRank> Rank = new Bindable<ScoreRank>(ScoreRank.X);

        /// <summary>
        /// THe highest combo achieved by this score.
        /// </summary>
        public readonly BindableInt HighestCombo = new BindableInt();

        /// <summary>
        /// Whether all <see cref="Judgement"/>s have been processed.
        /// </summary>
        protected virtual bool HasCompleted => false;

        /// <summary>
        /// Whether this ScoreProcessor has already triggered the failed state.
        /// </summary>
        public virtual bool HasFailed { get; private set; }

        /// <summary>
        /// The default conditions for failing.
        /// </summary>
        protected virtual bool DefaultFailCondition => Health.Value == Health.MinValue;

        protected ScoreProcessor()
        {
            Combo.ValueChanged += delegate { HighestCombo.Value = Math.Max(HighestCombo.Value, Combo.Value); };
            Accuracy.ValueChanged += delegate { Rank.Value = rankFrom(Accuracy.Value); };
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

            HasFailed = false;
        }

        /// <summary>
        /// Checks if the score is in a failed state and notifies subscribers.
        /// <para>
        /// This can only ever notify subscribers once.
        /// </para>
        /// </summary>
        protected void UpdateFailed()
        {
            if (HasFailed)
                return;

            if (!DefaultFailCondition && FailConditions?.Invoke(this) != true)
                return;

            if (Failed?.Invoke() != false)
                HasFailed = true;
        }

        /// <summary>
        /// Notifies subscribers of <see cref="NewJudgement"/> that a new judgement has occurred.
        /// </summary>
        /// <param name="judgement">The judgement to notify subscribers of.</param>
        protected void NotifyNewJudgement(Judgement judgement)
        {
            NewJudgement?.Invoke(judgement);

            if (HasCompleted)
                AllJudged?.Invoke();
        }

        /// <summary>
        /// Retrieve a score populated with data for the current play this processor is responsible for.
        /// </summary>
        public virtual void PopulateScore(Score score)
        {
            score.TotalScore = TotalScore;
            score.Combo = Combo;
            score.MaxCombo = HighestCombo;
            score.Accuracy = Accuracy;
            score.Rank = Rank;
            score.Date = DateTimeOffset.Now;
            score.Health = Health;
        }
    }

    public class ScoreProcessor<TObject> : ScoreProcessor
        where TObject : HitObject
    {
        private const double base_portion = 0.3;
        private const double combo_portion = 0.7;
        private const double max_score = 1000000;

        public readonly Bindable<ScoringMode> Mode = new Bindable<ScoringMode>();

        protected sealed override bool HasCompleted => JudgedHits == MaxHits;

        protected int MaxHits { get; private set; }
        protected int JudgedHits { get; private set; }

        private double maxHighestCombo;

        private double maxBaseScore;
        private double rollingMaxBaseScore;
        private double baseScore;
        private double bonusScore;

        protected ScoreProcessor()
        {
        }

        public ScoreProcessor(RulesetContainer<TObject> rulesetContainer)
        {
            Debug.Assert(base_portion + combo_portion == 1.0);

            rulesetContainer.OnJudgement += AddJudgement;
            rulesetContainer.OnJudgementRemoved += RemoveJudgement;

            SimulateAutoplay(rulesetContainer.Beatmap);
            Reset(true);

            if (maxBaseScore == 0 || maxHighestCombo == 0)
            {
                Mode.Value = ScoringMode.Exponential;
                Mode.Disabled = true;
            }
        }

        /// <summary>
        /// Simulates an autoplay of <see cref="HitObject"/>s that will be judged by this <see cref="ScoreProcessor{TObject}"/>
        /// by adding <see cref="Judgement"/>s for each <see cref="HitObject"/> in the <see cref="Beatmap{TObject}"/>.
        /// <para>
        /// This is required for <see cref="ScoringMode.Standardised"/> to work, otherwise <see cref="ScoringMode.Exponential"/> will be used.
        /// </para>
        /// </summary>
        /// <param name="beatmap">The <see cref="Beatmap{TObject}"/> containing the <see cref="HitObject"/>s that will be judged by this <see cref="ScoreProcessor{TObject}"/>.</param>
        protected virtual void SimulateAutoplay(Beatmap<TObject> beatmap) { }

        /// <summary>
        /// Adds a judgement to this ScoreProcessor.
        /// </summary>
        /// <param name="judgement">The judgement to add.</param>
        protected void AddJudgement(Judgement judgement)
        {
            OnNewJudgement(judgement);
            updateScore();

            UpdateFailed();
            NotifyNewJudgement(judgement);
        }

        protected void RemoveJudgement(Judgement judgement)
        {
            OnJudgementRemoved(judgement);
            updateScore();
        }

        /// <summary>
        /// Applies a judgement.
        /// </summary>
        /// <param name="judgement">The judgement to apply/</param>
        protected virtual void OnNewJudgement(Judgement judgement)
        {
            judgement.ComboAtJudgement = Combo;
            judgement.HighestComboAtJudgement = HighestCombo;

            if (judgement.AffectsCombo)
            {
                switch (judgement.Result)
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

                baseScore += judgement.NumericResult;
                rollingMaxBaseScore += judgement.MaxNumericResult;

                JudgedHits++;
            }
            else if (judgement.IsHit)
                bonusScore += judgement.NumericResult;
        }

        /// <summary>
        /// Removes a judgement. This should reverse everything in <see cref="OnNewJudgement(Judgement)"/>.
        /// </summary>
        /// <param name="judgement">The judgement to remove.</param>
        protected virtual void OnJudgementRemoved(Judgement judgement)
        {
            Combo.Value = judgement.ComboAtJudgement;
            HighestCombo.Value = judgement.HighestComboAtJudgement;

            if (judgement.AffectsCombo)
            {
                baseScore -= judgement.NumericResult;
                rollingMaxBaseScore -= judgement.MaxNumericResult;

                JudgedHits--;
            }
            else if (judgement.IsHit)
                bonusScore -= judgement.NumericResult;
        }

        private void updateScore()
        {
            if (rollingMaxBaseScore != 0)
                Accuracy.Value = baseScore / rollingMaxBaseScore;

            switch (Mode.Value)
            {
                case ScoringMode.Standardised:
                    TotalScore.Value = max_score * (base_portion * baseScore / maxBaseScore + combo_portion * HighestCombo / maxHighestCombo) + bonusScore;
                    break;
                case ScoringMode.Exponential:
                    TotalScore.Value = (baseScore + bonusScore) * Math.Log(HighestCombo + 1, 2);
                    break;
            }
        }

        protected override void Reset(bool storeResults)
        {
            if (storeResults)
            {
                MaxHits = JudgedHits;
                maxHighestCombo = HighestCombo;
                maxBaseScore = baseScore;
            }

            base.Reset(storeResults);

            JudgedHits = 0;
            baseScore = 0;
            rollingMaxBaseScore = 0;
            bonusScore = 0;
        }
    }

    public enum ScoringMode
    {
        Standardised,
        Exponential
    }
}
