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
    /// <summary>
    /// Used as an accuracy counter. Represented visually as a percentage, internally as a fraction.
    /// </summary>
    public class AccuracyCounter : RollingCounter<float>
    {
        protected override Type transformType => typeof(TransformAccuracy);

        private long numerator = 0;
        public long Numerator
        {
            get
            {
                return numerator;
            }
            set
            {
                numerator = value;
                updateCount();
            }
        }

        private ulong denominator = 0;
        public ulong Denominator
        {
            get
            {
                return denominator;
            }
            set
            {
                denominator = value;
                updateCount();
            }
        }

        public void SetCount(long num, ulong den)
        {
            numerator = num;
            denominator = den;
            updateCount();
        }

        private void updateCount()
        {
            Count = Denominator == 0 ? 100.0f : (Numerator * 100.0f) / Denominator;
        }

        public override void ResetCount()
        {
            numerator = 0;
            denominator = 0;
            updateCount();
            StopRolling();
        }

        protected override string formatCount(float count)
        {
            return count.ToString("0.00") + "%";
        }

        protected class TransformAccuracy : Transform<float>
        {
            public override float CurrentValue
            {
                get
                {
                    double time = Time;
                    if (time < StartTime) return StartValue;
                    if (time >= EndTime) return EndValue;

                    return Interpolation.ValueAt(time, StartValue, EndValue, StartTime, EndTime, Easing);
                }
            }

            public override void Apply(Drawable d)
            {
                base.Apply(d);
                (d as AccuracyCounter).VisibleCount = CurrentValue;
            }

            public TransformAccuracy(IClock clock)
                : base(clock)
            {
            }
        }
    }
}
