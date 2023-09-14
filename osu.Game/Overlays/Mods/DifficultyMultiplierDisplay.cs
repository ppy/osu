// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Mods
{
    public sealed partial class DifficultyMultiplierDisplay : ModCounterDisplay
    {
        protected override LocalisableString Label => DifficultyMultiplierDisplayStrings.DifficultyMultiplier;

        protected override string CounterFormat => @"0.0x";

        public DifficultyMultiplierDisplay()
        {
            Current.Default = 1d;
            Current.Value = 1d;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // required to prevent the counter initially rolling up from 0 to 1
            // due to `Current.Value` having a nonstandard default value of 1.
            Counter.SetCountWithoutRolling(Current.Value);
        }
    }
}
