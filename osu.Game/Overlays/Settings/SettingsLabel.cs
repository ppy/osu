// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Settings
{
    public class SettingsLabel : SettingsItem<string>
    {
        protected override Drawable CreateControl() => null;

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            Colour = colour.Gray6;
        }
    }
}
