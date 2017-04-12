// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Framework.MathUtils;
using System;
using osu.Framework.Allocation;

namespace osu.Game.Graphics.UserInterface
{
    public class ScoreCounter : RollingCounter<double>
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

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            DisplayedCountSpriteText.Colour = colours.BlueLighter;
        }

        protected override double GetProportionalDuration(double currentValue, double newValue)
        {
            return currentValue > newValue ? currentValue - newValue : newValue - currentValue;
        }

        protected override string FormatCount(double count)
        {
            string format = new string('0', (int)LeadingZeroes);
            for (int i = format.Length - 3; i > 0; i -= 3)
                format = format.Insert(i, @",");

            return ((long)count).ToString(format);
        }

        public override void Increment(double amount)
        {
            Current.Value = Current + amount;
        }

        protected class TransformScore : Transform<double>
        {
            public override double CurrentValue
            {
                get
                {
                    double time = Time?.Current ?? 0;
                    if (time < StartTime) return StartValue;
                    if (time >= EndTime) return EndValue;

                    return Interpolation.ValueAt(time, (float)StartValue, (float)EndValue, StartTime, EndTime, Easing);
                }
            }

            public override void Apply(Drawable d)
            {
                base.Apply(d);
                ((ScoreCounter)d).DisplayedCount = CurrentValue;
            }
        }
    }
}
