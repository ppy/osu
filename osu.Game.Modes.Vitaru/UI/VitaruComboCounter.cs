//Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Modes.UI;

namespace osu.Game.Modes.Vitaru.UI
{
    public class VitaruComboCounter : ComboCounter
    {
        protected override void OnDisplayedCountChange(int newValue)
        {
            throw new NotImplementedException();
        }

        protected override void OnDisplayedCountIncrement(int newValue)
        {
            throw new NotImplementedException();
        }

        protected override void OnDisplayedCountRolling(int currentValue, int newValue)
        {
            throw new NotImplementedException();
        }
    }
}
