// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Configuration;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Beatmaps;

namespace osu.Game.Modes
{
    public abstract class ScoreProcessor
    {
        public virtual Score GetScore() => new Score()
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
        public ScoreProcessor(int hitObjectCount = 0)
        {
            Combo.ValueChanged += delegate { HighestCombo.Value = Math.Max(HighestCombo.Value, Combo.Value); };
            Judgements = new List<JudgementInfo>(hitObjectCount);
        }

        public void AddJudgement(JudgementInfo judgement)
        {
            Judgements.Add(judgement);

            UpdateCalculations(judgement);

            judgement.ComboAtHit = (ulong)Combo.Value;

            CheckFailed();
        }

        /// <summary>
        /// Initialize final values by simulating an auto-play of the beatmap.
        /// </summary>
        /// <param name="beatmap">The beatmap to initialize calculations with.</param>
        public abstract void Initialize(Beatmap beatmap);

        /// <summary>
        /// Rests the score processor to a stale state.
        /// </summary>
        public virtual void Reset()
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
        public void CheckFailed()
        {
            if (!hasFailed && ShouldFail)
            {
                hasFailed = true;
                Failed?.Invoke();
            }
        }
    }
}
