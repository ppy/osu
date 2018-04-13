// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Configuration;

namespace osu.Game.Screens.Edit.Screens.Compose
{
    public class BindableBeatDivisor : BindableNumber<int>
    {
        private DivisorPanelType panel = DivisorPanelType.Normal;
        public static readonly int[] VALID_DIVISORS = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 12, 16, 18, 24, 32 };
        public static readonly int[] VALID_NORMAL_DIVISORS = { 1, 2, 4, 8, 16, 32 };
        public static readonly int[] VALID_TRIPLET_DIVISORS = { 1, 3, 6, 9, 12, 18, 24 };
        public static readonly int[] VALID_OTHER_DIVISORS = { 1, 5, 7, 11 };

        /// <summary>An event that occurs when the <see cref="ValidDivisors"/> property has changed.</summary>
        public event Action ValidDivisorsChanged;

        public BindableBeatDivisor(int value = 1)
            : base(value)
        {
        }

        /// <summary>Triggers the <see cref="ValidDivisorsChanged"/> event.</summary>
        public virtual void TriggerValidDivisorsChanged()
        {
            ValidDivisorsChanged?.Invoke();
        }

        public void NextPanel()
        {
            Panel = (DivisorPanelType)((int)(panel + 1) % 3);
            //switch (panel) // Should I use a normal if-else statement?
            //{
            //    case DivisorPanelType.Normal:
            //        Panel = DivisorPanelType.Triplets;
            //        return;
            //    case DivisorPanelType.Triplets:
            //        Panel = DivisorPanelType.Other;
            //        return;
            //    case DivisorPanelType.Other:
            //        Panel = DivisorPanelType.Normal;
            //        return;
            //    default:
            //        return;
            //}
        }

        public void PreviousPanel()
        {
            Panel = (DivisorPanelType)((int)(panel + 2) % 3);
            //switch (panel) // Should I use a normal if-else statement?
            //{
            //    case DivisorPanelType.Normal:
            //        Panel = DivisorPanelType.Other;
            //        return;
            //    case DivisorPanelType.Triplets:
            //        Panel = DivisorPanelType.Normal;
            //        return;
            //    case DivisorPanelType.Other:
            //        Panel = DivisorPanelType.Triplets;
            //        return;
            //    default:
            //        return;
            //}
        }

        public void NextDivisor()
        {
            switch (panel)
            {
                case DivisorPanelType.Normal:
                    NextNormalDivisor();
                    return;
                case DivisorPanelType.Triplets:
                    NextTripletDivisor();
                    return;
                case DivisorPanelType.Other:
                    NextOtherDivisor();
                    return;
                default:
                    return;
            }
        }

        public void PreviousDivisor()
        {
            switch (panel)
            {
                case DivisorPanelType.Normal:
                    PreviousNormalDivisor();
                    return;
                case DivisorPanelType.Triplets:
                    PreviousTripletDivisor();
                    return;
                case DivisorPanelType.Other:
                    PreviousOtherDivisor();
                    return;
                default:
                    return;
            }
        }

        public void NextNormalDivisor() => Value = VALID_NORMAL_DIVISORS[Math.Min(VALID_NORMAL_DIVISORS.Length - 1, Array.IndexOf(VALID_NORMAL_DIVISORS, Value) + 1)];

        public void PreviousNormalDivisor() => Value = VALID_NORMAL_DIVISORS[Math.Max(0, Array.IndexOf(VALID_NORMAL_DIVISORS, Value) - 1)];

        public void NextTripletDivisor() => Value = VALID_TRIPLET_DIVISORS[Math.Min(VALID_TRIPLET_DIVISORS.Length - 1, Array.IndexOf(VALID_TRIPLET_DIVISORS, Value) + 1)];

        public void PreviousTripletDivisor() => Value = VALID_TRIPLET_DIVISORS[Math.Max(0, Array.IndexOf(VALID_TRIPLET_DIVISORS, Value) - 1)];

        public void NextOtherDivisor() => Value = VALID_OTHER_DIVISORS[Math.Min(VALID_OTHER_DIVISORS.Length - 1, Array.IndexOf(VALID_OTHER_DIVISORS, Value) + 1)];

        public void PreviousOtherDivisor() => Value = VALID_OTHER_DIVISORS[Math.Max(0, Array.IndexOf(VALID_OTHER_DIVISORS, Value) - 1)];

        public override int Value
        {
            get => base.Value;
            set
            {
                if (!VALID_DIVISORS.Contains(value))
                    throw new ArgumentOutOfRangeException($"Provided divisor is not in {nameof(VALID_DIVISORS)}");

                base.Value = value;
            }
        }

        public DivisorPanelType Panel
        {
            get => panel;
            set
            {
                panel = value;
                base.Value = 1; // Reset the value
                switch (panel)
                {
                    case DivisorPanelType.Normal:
                        ValidDivisors = VALID_NORMAL_DIVISORS;
                        goto default;
                    case DivisorPanelType.Triplets:
                        ValidDivisors = VALID_TRIPLET_DIVISORS;
                        goto default;
                    case DivisorPanelType.Other:
                        ValidDivisors = VALID_OTHER_DIVISORS;
                        goto default;
                    default:
                        TriggerValidDivisorsChanged();
                        return;
                }
                //Panel = value;
            }
        }

        public int[] ValidDivisors = VALID_NORMAL_DIVISORS;

        protected override int DefaultMinValue => VALID_DIVISORS.First();
        protected override int DefaultMaxValue => VALID_DIVISORS.Last();
        protected override int DefaultPrecision => 1;
    }

    public enum DivisorPanelType
    {
        Normal = 0,
        Triplets = 1,
        Other = 2
    }
}
