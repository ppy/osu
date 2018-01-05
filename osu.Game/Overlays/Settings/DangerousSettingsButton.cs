// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Settings
{
    /// <summary>
    /// A <see cref="SettingsButton"/> with pink colours to mark dangerous/destructive actions.
    /// </summary>
    public class DangerousSettingsButton : SettingsButton
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundColour = colours.Pink;

            Triangles.ColourDark = colours.PinkDark;
            Triangles.ColourLight = colours.PinkLight;
        }
    }
}
