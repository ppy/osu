// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Configuration;

namespace osu.Game.Screens.Edit.Screens.Compose
{
    public class BindableBeatDivisor : BindableNumber<int>
    {
        public static readonly int[] VALID_DIVISORS = { 1, 2, 3, 4, 6, 8, 12, 16 };

        public BindableBeatDivisor(int value = 1)
            : base(value)
        {
        }

        public void Next() => Value = VALID_DIVISORS[Math.Min(VALID_DIVISORS.Length - 1, Array.IndexOf(VALID_DIVISORS, Value) + 1)];

        public void Previous() => Value = VALID_DIVISORS[Math.Max(0, Array.IndexOf(VALID_DIVISORS, Value) - 1)];

        public override int Value
        {
            get { return base.Value; }
            set
            {
                int snapped = 1;

                for (int i = 1; i < VALID_DIVISORS.Length; i++)
                {
                    var curr = VALID_DIVISORS[i];
                    var prev = VALID_DIVISORS[i - 1];
                    if (value < prev + (curr - prev) / 2f)
                    {
                        snapped = prev;
                        break;
                    }

                    snapped = curr;
                }

                if (snapped == Value)
                    // it may be that we are already at the snapped value, but we want bound components to still be made aware that we possibly modified an incoming ValueChanged.
                    TriggerValueChange();
                else
                    base.Value = snapped;
            }
        }

        protected override int DefaultMinValue => VALID_DIVISORS.First();
        protected override int DefaultMaxValue => VALID_DIVISORS.Last();
        protected override int DefaultPrecision => 1;
    }
}
