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
    public class ScoreProcessor
    {
        public virtual Score GetScore() => new Score();

        public BindableDouble TotalScore = new BindableDouble { MinValue = 0 };

        public BindableDouble Accuracy = new BindableDouble { MinValue = 0, MaxValue = 1 };

        public BindableInt Combo = new BindableInt();

        public List<JudgementInfo> Judgements = new List<JudgementInfo>();

        public virtual void AddJudgement(JudgementInfo judgement)
        {
            Judgements.Add(judgement);
            UpdateCalculations();
        }

        /// <summary>
        /// Update any values that potentially need post-processing on a judgement change.
        /// </summary>
        protected virtual void UpdateCalculations()
        {
            
        }
    }
}
