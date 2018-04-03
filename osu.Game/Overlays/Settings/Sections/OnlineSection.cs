// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Settings.Sections
{
    public class OnlineSection : SettingsSection
    {
        public override string Header => "Online";
        public override FontAwesome Icon => FontAwesome.fa_globe;

        public OnlineSection()
        {
            Children = new Drawable[]
            {
            };
        }
    }
}
