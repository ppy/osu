// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// Used to display combo with a roll-up animation in results screen.
    /// </summary>
    public class ComboResultCounter : RollingCounter<long>
    {
        protected override double RollingDuration => 500;
        protected override Easing RollingEasing => Easing.Out;

        protected override double GetProportionalDuration(long currentValue, long newValue)
        {
            return currentValue > newValue ? currentValue - newValue : newValue - currentValue;
        }

        protected override string FormatCount(long count)
        {
            return $@"{count}x";
        }

        public override void Increment(long amount)
        {
            Current.Value = Current + amount;
        }
    }
}
