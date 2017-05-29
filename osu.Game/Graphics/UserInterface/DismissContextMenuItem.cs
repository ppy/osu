// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;

namespace osu.Game.Graphics.UserInterface
{
    public class DismissContextMenuItem : ContextMenuItem
    {
        public DismissContextMenuItem(string title) : base(title)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            TextColour = colours.Red;
        }
    }
}
