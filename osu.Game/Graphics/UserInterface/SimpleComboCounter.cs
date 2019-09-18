// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// Used as an accuracy counter. Represented visually as a percentage.
    /// </summary>
    public class SimpleComboCounter : RollingCounter<int>
    {
        protected override double RollingDuration => 750;

        public SimpleComboCounter()
        {
            Current.Value = DisplayedCount = 0;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours) => AccentColour = colours.BlueLighter;

        protected override string FormatCount(int count)
        {
            return $@"{count}x";
        }

        protected override double GetProportionalDuration(int currentValue, int newValue)
        {
            return Math.Abs(currentValue - newValue) * RollingDuration * 100.0f;
        }

        public override void Increment(int amount)
        {
            Current.Value = Current.Value + amount;
        }
    }
}
