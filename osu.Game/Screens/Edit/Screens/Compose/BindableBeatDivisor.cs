// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;

namespace osu.Game.Screens.Edit.Screens.Compose
{
    public class BindableBeatDivisor : Bindable<int>
    {
        public BindableBeatDivisor(int value = 1)
            : base(value)
        {
        }
    }
}
