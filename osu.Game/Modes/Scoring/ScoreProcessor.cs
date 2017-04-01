// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Game.Beatmaps;
using osu.Game.Modes.Judgements;
using osu.Game.Modes.Objects;
using osu.Game.Modes.UI;

namespace osu.Game.Modes.Scoring
{
    public abstract class ScoreProcessor
    {
        /// <summary>
        /// Invoked when the ScoreProcessor is in a failed state.
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

        /// <summary>
        /// Whether the score is in a failed state.
        /// </summary>
        public virtual bool HasFailed => false;

        /// <summary>
        /// Whether this ScoreProcessor has already triggered the failed state.
        /// </summary>
        private bool alreadyFailed;

        protected ScoreProcessor()
        {
            Combo.ValueChanged += delegate { HighestCombo.Value = Math.Max(HighestCombo.Value, Combo.Value); };

            Reset();
        }

        /// <summary>
        /// Creates a Score applicable to the game mode in which this ScoreProcessor resides.
        /// </summary>
        /// <returns>The Score.</returns>
        public virtual Score CreateScore() => new Score
        {
            TotalScore = TotalScore,
            Combo = Combo,
            MaxCombo = HighestCombo,
            Accuracy = Accuracy,
            Health = Health,
        };

        /// <summary>
        /// Resets this ScoreProcessor to a default state.
        /// </summary>
        protected virtual void Reset()
        {
            TotalScore.Value = 0;
            Accuracy.Value = 0;
            Health.Value = 0;
            Combo.Value = 0;
            HighestCombo.Value = 0;

            alreadyFailed = false;
        }

        /// <summary>
        /// Checks if the score is in a failed state and notifies subscribers.
        /// <para>
        /// This can only ever notify subscribers once.
        /// </para>
        /// </summary>
        protected void UpdateFailed()
        {
            if (alreadyFailed || !HasFailed)
                return;

            alreadyFailed = true;
            Failed?.Invoke();
        }
    }

    public abstract class ScoreProcessor<TObject, TJudgement> : ScoreProcessor
        where TObject : HitObject
        where TJudgement : Judgement
    {
        /// <summary>
        /// All judgements held by this ScoreProcessor.
        /// </summary>
        protected readonly List<TJudgement> Judgements = new List<TJudgement>();

        public override bool HasFailed => Health.Value == Health.MinValue;

        protected ScoreProcessor()
        {
        }

        protected ScoreProcessor(HitRenderer<TObject, TJudgement> hitRenderer)
        {
            Judgements.Capacity = hitRenderer.Beatmap.HitObjects.Count;

            hitRenderer.OnJudgement += AddJudgement;

            ComputeTargets(hitRenderer.Beatmap);

            Reset();
        }

        /// <summary>
        /// Computes target scoring values for this ScoreProcessor. This is equivalent to performing an auto-play of the score to find the values.
        /// </summary>
        /// <param name="beatmap">The Beatmap containing the objects that will be judged by this ScoreProcessor.</param>
        protected virtual void ComputeTargets(Beatmap<TObject> beatmap) { }

        /// <summary>
        /// Adds a judgement to this ScoreProcessor.
        /// </summary>
        /// <param name="judgement">The judgement to add.</param>
        protected void AddJudgement(TJudgement judgement)
        {
            bool exists = Judgements.Contains(judgement);

            if (!exists)
            {
                Judgements.Add(judgement);
                OnNewJudgement(judgement);

                judgement.ComboAtHit = Combo.Value;
            }
            else
                OnJudgementChanged(judgement);

            UpdateFailed();
        }

        protected override void Reset()
        {
            base.Reset();

            Judgements.Clear();
        }

        /// <summary>
        /// Updates any values that need post-processing. Invoked when a new judgement has occurred.
        /// <para>
        /// This is not triggered when existing judgements are changed - for that see <see cref="OnJudgementChanged(TJudgement)"/>.
        /// </para>
        /// </summary>
        /// <param name="judgement">The judgement that triggered this calculation.</param>
        protected abstract void OnNewJudgement(TJudgement judgement);

        /// <summary>
        /// Updates any values that need post-processing. Invoked when an existing judgement has changed.
        /// <para>
        /// This is not triggered when a new judgement has occurred - for that see <see cref="OnNewJudgement(TJudgement)"/>.
        /// </para>
        /// </summary>
        /// <param name="judgement">The judgement that triggered this calculation.</param>
        protected virtual void OnJudgementChanged(TJudgement judgement) { }
    }
}