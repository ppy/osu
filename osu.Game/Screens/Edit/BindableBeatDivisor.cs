// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Colour;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit
{
    public class BindableBeatDivisor : BindableInt
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
            get => base.Value;
            set
            {
                if (!VALID_DIVISORS.Contains(value))
                    throw new ArgumentOutOfRangeException($"Provided divisor is not in {nameof(VALID_DIVISORS)}");

                base.Value = value;
            }
        }

        protected override int DefaultMinValue => VALID_DIVISORS.First();
        protected override int DefaultMaxValue => VALID_DIVISORS.Last();
        protected override int DefaultPrecision => 1;

        /// <summary>
        /// Retrieves the appropriate colour for a beat divisor.
        /// </summary>
        /// <param name="beatDivisor">The beat divisor.</param>
        /// <param name="colours">The set of colours.</param>
        /// <returns>The applicable colour from <paramref name="colours"/> for <paramref name="beatDivisor"/>.</returns>
        public static ColourInfo GetColourFor(int beatDivisor, OsuColour colours)
        {
            switch (beatDivisor)
            {
                case 2:
                    return colours.BlueLight;

                case 4:
                    return colours.Blue;

                case 8:
                    return colours.BlueDarker;

                case 16:
                    return colours.PurpleDark;

                case 3:
                    return colours.YellowLight;

                case 6:
                    return colours.Yellow;

                case 12:
                    return colours.YellowDarker;

                default:
                    return Color4.White;
            }
        }
    }
}
