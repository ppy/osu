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
                {
                    // If it doesn't match, value will be 0, but will be clamped to the valid range via DefaultMinValue
                    value = Array.FindLast(VALID_DIVISORS, d => d < value);
                }

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
            => beatDivisor switch
            {
                2 => colours.BlueLight,
                4 => colours.Blue,
                8 => colours.BlueDarker,
                16 => colours.PurpleDark,

                3 => colours.YellowLight,
                6 => colours.Yellow,
                12 => colours.YellowDarker,

                _ => Color4.White,
            };
    }
}
