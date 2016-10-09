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
    /// A simple rolling counter that accepts unsigned long values.
    /// </summary>
    public class ULongCounter : NumericRollingCounter<ulong>
    {
        protected override Type transformType => typeof(TransformULongCounter);

        public override void ResetCount()
        {
            SetCountWithoutRolling(0);
        }

        protected override string formatCount(ulong count)
        {
            return count.ToString("#,0");
        }

        protected class TransformULongCounter : Transform<ulong>
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
                (d as ULongCounter).VisibleCount = CurrentValue;
            }

            public TransformULongCounter(IClock clock)
                : base(clock)
            {
            }
        }
    }
}
