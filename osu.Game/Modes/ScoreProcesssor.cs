//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Configuration;
using osu.Game.Modes.Objects.Drawables;

namespace osu.Game.Modes
{
    public abstract class ScoreProcessor
    {
        public virtual Score GetScore() => new Score();

        public readonly BindableDouble TotalScore = new BindableDouble { MinValue = 0 };

        public readonly BindableDouble Accuracy = new BindableDouble { MinValue = 0, MaxValue = 1 };

        public readonly BindableInt Combo = new BindableInt();

        public readonly BindableInt MaximumCombo = new BindableInt();

        public readonly List<JudgementInfo> Judgements = new List<JudgementInfo>();

        public ScoreProcessor()
        {
            Combo.ValueChanged += delegate { MaximumCombo.Value = Math.Max(MaximumCombo.Value, Combo.Value); };
        }

        public void AddJudgement(JudgementInfo judgement)
        {
            Judgements.Add(judgement);

            UpdateCalculations(judgement);

            judgement.ComboAtHit = (ulong)Combo.Value;
        }

        /// <summary>
        /// Update any values that potentially need post-processing on a judgement change.
        /// </summary>
        /// <param name="newJudgement">A new JudgementInfo that triggered this calculation. May be null.</param>
        protected abstract void UpdateCalculations(JudgementInfo newJudgement);
    }
}
