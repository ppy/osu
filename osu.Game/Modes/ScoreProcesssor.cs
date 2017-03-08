// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Beatmaps;

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
        /// Whether the player is in a failable state.
        /// </summary>
        protected abstract bool ShouldFail { get; }

        /// <summary>
        /// Called when we reach a failing health of zero.
        /// </summary>
        public event Action Failed;

        /// <summary>
        /// Keeps track of the highest combo ever achieved in this play.
        /// This is handled automatically by ScoreProcessor.
        /// </summary>
        public readonly BindableInt HighestCombo = new BindableInt();

        public readonly List<JudgementInfo> Judgements;

        private bool hasFailed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScoreProcessor"/> class.
        /// </summary>
        /// <param name="hitObjectCount">Number of HitObjects. It is used for specifying Judgements collection Capacity</param>
        protected ScoreProcessor(Beatmap beatmap = null)
        {
            Combo.ValueChanged += delegate { HighestCombo.Value = Math.Max(HighestCombo.Value, Combo.Value); };

            if (beatmap != null)
            {
                Judgements = new List<JudgementInfo>(beatmap.HitObjects.Count);

                CalculateFinalValues(beatmap);
                Reset();
            }
        }

        /// <summary>
        /// Adds a judgement to this processor.
        /// </summary>
        /// <param name="judgement">The judgement to add.</param>
        public void AddJudgement(JudgementInfo judgement)
        {
            Judgements.Add(judgement);

            UpdateCalculations(judgement);

            judgement.ComboAtHit = (ulong)Combo.Value;

            CheckFailed();
        }

        /// <summary>
        /// Calculates and stores final scoring values by simulating an auto-play of the beatmap.
        /// </summary>
        /// <param name="beatmap">The beatmap to initialize calculations with.</param>
        protected abstract void CalculateFinalValues(Beatmap beatmap);

        /// <summary>
        /// Rests the processor to a stale state. Should revert any variables changed through the
        /// <see cref="CalculateFinalValues(Beatmap)"/> method.
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
        protected abstract void UpdateCalculations(JudgementInfo newJudgement);

        /// <summary>
        /// Checks if the player has failed.
        /// </summary>
        /// <returns>Whether the player has failed.</returns>
        public bool CheckFailed()
        {
            if (!hasFailed && ShouldFail)
            {
                hasFailed = true;
                Failed?.Invoke();
            }

            return hasFailed;
        }
    }
}
