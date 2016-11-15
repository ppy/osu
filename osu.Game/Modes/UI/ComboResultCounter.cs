//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Framework.MathUtils;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Modes.UI
{
    /// <summary>
    /// Used to display combo with a roll-up animation in results screen.
    /// </summary>
    public class ComboResultCounter : RollingCounter<ulong>
    {
        protected override Type TransformType => typeof(TransformComboResult);

        protected override double RollingDuration => 500;
        protected override EasingTypes RollingEasing => EasingTypes.Out;

        protected override double GetProportionalDuration(ulong currentValue, ulong newValue)
        {
            return currentValue > newValue ? currentValue - newValue : newValue - currentValue;
        }

        protected override string FormatCount(ulong count)
        {
            return $@"{count}x";
        }

        public override void Increment(ulong amount)
        {
            Count = Count + amount;
        }

        protected class TransformComboResult : Transform<ulong>
        {
            protected override ulong CurrentValue
            {
                get
                {
                    double time = Time?.Current ?? 0;
                    if (time < StartTime) return StartValue;
                    if (time >= EndTime) return EndValue;

                    return (ulong)Interpolation.ValueAt(time, StartValue, EndValue, StartTime, EndTime, Easing);
                }
            }

            public override void Apply(Drawable d)
            {
                base.Apply(d);
                (d as ComboResultCounter).DisplayedCount = CurrentValue;
            }
        }
    }
}
