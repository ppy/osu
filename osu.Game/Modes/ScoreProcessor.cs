// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using System;
using System.Collections.Generic;
using osu.Game.Modes.Judgements;
using osu.Game.Modes.UI;
using osu.Game.Modes.Objects;

namespace osu.Game.Modes
{
    public abstract class ScoreProcessor
    {
        /// <summary>
        /// Invoked when the score is in a failing state.
        /// </summary>
        public event Action Failed;

        /// <summary>
        /// The current total score.
        /// </summary>
        public readonly BindableDouble TotalScore = new BindableDouble { MinValue = 0 };

        /// <summary>
        /// The current accuracy.
        /// </summary>
        public readonly BindableDouble Accuracy = new BindableDouble { MinValue = 0, MaxValue = 1 };

        /// <summary>
        /// The current health.
        /// </summary>
        public readonly BindableDouble Health = new BindableDouble { MinValue = 0, MaxValue = 1 };

        /// <summary>
        /// The current combo.
        /// </summary>
        public readonly BindableInt Combo = new BindableInt();

        /// <summary>
        /// THe highest combo achieved by this score.
        /// </summary>
        public readonly BindableInt HighestCombo = new BindableInt();

        public virtual Score GetScore() => new Score
        {
            TotalScore = TotalScore,
            Combo = Combo,
            MaxCombo = HighestCombo,
            Accuracy = Accuracy,
            Health = Health,
        };

        /// <summary>
        /// Checks if the score is in a failing state.
        /// </summary>
        /// <returns>Whether the score is in a failing state.</returns>
        public abstract bool CheckFailed();

        /// <summary>
        /// Notifies subscribers that the score is in a failed state.
        /// </summary>
        protected void TriggerFailed()
        {
            Failed?.Invoke();
        }
    }

    public abstract class ScoreProcessor<TObject, TJudgement> : ScoreProcessor
        where TObject : HitObject
        where TJudgement : JudgementInfo
    {
        /// <summary>
        /// All judgements held by this ScoreProcessor.
        /// </summary>
        protected readonly List<TJudgement> Judgements = new List<TJudgement>();

        /// <summary>
        /// Whether the score is in a failable state.
        /// </summary>
        protected virtual bool IsFailable => Health.Value == Health.MinValue;

        /// <summary>
        /// Whether this ScoreProcessor has already failed.
        /// </summary>
        private bool hasFailed;

        protected ScoreProcessor()
        {
            Combo.ValueChanged += delegate { HighestCombo.Value = Math.Max(HighestCombo.Value, Combo.Value); };

            Reset();
        }

        protected ScoreProcessor(HitRenderer<TObject, TJudgement> hitRenderer)
            : this()
        {
            Judgements.Capacity = hitRenderer.Beatmap.HitObjects.Count;
            hitRenderer.OnJudgement += addJudgement;
        }

        /// <summary>
        /// Adds a judgement to this ScoreProcessor.
        /// </summary>
        /// <param name="judgement">The judgement to add.</param>
        private void addJudgement(TJudgement judgement)
        {
            Judgements.Add(judgement);

            UpdateCalculations(judgement);

            judgement.ComboAtHit = (ulong)Combo.Value;

            CheckFailed();
        }

        public override bool CheckFailed()
        {
            if (!hasFailed && IsFailable)
            {
                hasFailed = true;
                TriggerFailed();
            }

            return hasFailed;
        }

        /// <summary>
        /// Resets this ScoreProcessor to a stale state.
        /// </summary>
        protected virtual void Reset()
        {
            Judgements.Clear();

            hasFailed = false;
            TotalScore.Value = 0;
            Accuracy.Value = 0;
            Health.Value = 0;
            Combo.Value = 0;
            HighestCombo.Value = 0;
        }

        /// <summary>
        /// Update any values that potentially need post-processing on a judgement change.
        /// </summary>
        /// <param name="newJudgement">A new JudgementInfo that triggered this calculation. May be null.</param>
        protected abstract void UpdateCalculations(TJudgement newJudgement);
    }
}
