// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class OptionTextBox : OsuTextBox
    {
        private Bindable<string> bindable;

        public Bindable<string> Bindable
        {
            set
            {
                bindable = value;
                Current.BindTo(bindable);
            }
        }
    }
}
