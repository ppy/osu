// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Framework.Graphics;
using OpenTK.Graphics;
using osu.Framework.Allocation;

namespace osu.Game.Screens.Play
{
    public class ModsContainer : FillFlowContainer
    {
        private bool showMods;

        public bool ShowMods
        {
            get
            {
                return showMods;
            }

            set
            {
                if (showMods == value) return;

                showMods = value;
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Direction = FillDirection.Horizontal;

            if (ShowMods)
                Show();
            else
                Hide();
        }

        public void AddMod(Mod mod, Color4 colour)
        {
            Add(new ModIcon
            {
                AutoSizeAxes = Axes.Both,
                Icon = mod.Icon,
                Colour = colour,
                IconSize = 60,
            });
        }
    }
}
