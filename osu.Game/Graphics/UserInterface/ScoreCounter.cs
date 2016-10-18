//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Framework.MathUtils;
using osu.Framework.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Graphics.UserInterface
{
    public class ScoreCounter : RollingCounter<ulong>
    {
        protected override Type TransformType => typeof(TransformScore);

        protected override double RollingDuration => 1000;
        protected override EasingTypes RollingEasing => EasingTypes.Out;

        /// <summary>
        /// How many leading zeroes the counter has.
        /// </summary>
        public uint LeadingZeroes
        {
            get;
            protected set;
        }

        /// <summary>
        /// Displays score.
        /// </summary>
        /// <param name="leading">How many leading zeroes the counter will have.</param>
        public ScoreCounter(uint leading = 0)
        {
            DisplayedCountSpriteText.FixedWidth = true;
            LeadingZeroes = leading;
        }

        protected override double GetProportionalDuration(ulong currentValue, ulong newValue)
        {
            return currentValue > newValue ? currentValue - newValue : newValue - currentValue;
        }

        protected override string FormatCount(ulong count)
        {
            return count.ToString("D" + LeadingZeroes);
        }

        protected class TransformScore : Transform<ulong>
        {
            public override ulong CurrentValue
            {
                get
                {
                    double time = Time;
                    if (time < StartTime) return StartValue;
                    if (time >= EndTime) return EndValue;

                    return (ulong)Interpolation.ValueAt(time, StartValue, EndValue, StartTime, EndTime, Easing);
                }
            }

            public override void Apply(Drawable d)
            {
                base.Apply(d);
                (d as ScoreCounter).DisplayedCount = CurrentValue;
            }

            public TransformScore(IClock clock)
                : base(clock)
            {
            }
        }
    }
}
