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
        public virtual Score GetScore() => new Score
        {
            TotalScore = TotalScore,
            Combo = Combo,
            MaxCombo = HighestCombo,
            Accuracy = Accuracy,
            Health = Health,
        };

        public readonly BindableDouble TotalScore = new BindableDouble { MinValue = 0 };

        public readonly BindableDouble Accuracy = new BindableDouble { MinValue = 0, MaxValue = 1 };

        public readonly BindableDouble Health = new BindableDouble { MinValue = 0, MaxValue = 1 };

        public readonly BindableInt Combo = new BindableInt();

        /// <summary>
        /// Keeps track of the highest combo ever achieved in this play.
        /// This is handled automatically by ScoreProcessor.
        /// </summary>
        public readonly BindableInt HighestCombo = new BindableInt();

        /// <summary>
        /// Called when we reach a failing health of zero.
        /// </summary>
        public event Action Failed;

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
        protected List<TJudgement> Judgements;

        /// <summary>
        /// Are we allowed to fail?
        /// </summary>
        protected bool CanFail => true;

        /// <summary>
        /// Whether this ScoreProcessor has already triggered the failed event.
        /// </summary>
        protected bool HasFailed { get; private set; }

        protected ScoreProcessor()
        {
            Combo.ValueChanged += delegate { HighestCombo.Value = Math.Max(HighestCombo.Value, Combo.Value); };

            Reset();
        }

        protected ScoreProcessor(HitRenderer<TObject, TJudgement> hitRenderer)
            : this()
        {
            Judgements = new List<TJudgement>(hitRenderer.Beatmap.HitObjects.Count);
            hitRenderer.OnJudgement += addJudgement;
        }

        private void addJudgement(TJudgement judgement)
        {
            Judgements.Add(judgement);

            UpdateCalculations(judgement);

            judgement.ComboAtHit = (ulong)Combo.Value;
            if (Health.Value == Health.MinValue && !HasFailed)
            {
                HasFailed = true;
                TriggerFailed();
            }
        }

        /// <summary>
        /// Resets this ScoreProcessor to a stale state.
        /// </summary>
        protected virtual void Reset() { }

        /// <summary>
        /// Update any values that potentially need post-processing on a judgement change.
        /// </summary>
        /// <param name="newJudgement">A new JudgementInfo that triggered this calculation. May be null.</param>
        protected abstract void UpdateCalculations(TJudgement newJudgement);
    }
}
