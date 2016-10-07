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
    /// A simple rollover counter that accepts unsigned long values.
    /// </summary>
    public class ULongCounter : RollingCounter<ulong>
    {
        protected override void transformCount(ulong currentValue, ulong newValue)
        {
            transformCount(new TranformULongCounter(Clock), currentValue, newValue);
        }

        public override void ResetCount()
        {
            SetCountWithoutRolling(0);
        }

        protected override string formatCount(ulong count)
        {
            return count.ToString("#,0");
        }

        protected class TranformULongCounter : Transform<ulong>
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

            public TranformULongCounter(IClock clock)
                : base(clock)
            {
            }
        }
    }
}
