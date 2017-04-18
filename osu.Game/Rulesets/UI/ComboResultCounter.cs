// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Framework.MathUtils;
using osu.Game.Graphics.UserInterface;
using System;

namespace osu.Game.Rulesets.UI
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
            Current.Value = Current + amount;
        }

        protected class TransformComboResult : Transform<ulong>
        {
            public override ulong CurrentValue
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
                ((ComboResultCounter)d).DisplayedCount = CurrentValue;
            }
        }
    }
}
