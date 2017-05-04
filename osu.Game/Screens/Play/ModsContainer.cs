// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK;
using System.Collections.Generic;
using osu.Framework.Configuration;

namespace osu.Game.Screens.Play
{
    public class ModsContainer : Container
    {
        private readonly FillFlowContainer<ModIcon> iconsContainer;

        public readonly Bindable<IEnumerable<Mod>> Mods = new Bindable<IEnumerable<Mod>>();

        private bool showMods;
        public bool ShowMods
        {
            get
            {
                return showMods;
            }
            set
            {
                showMods = value;
                if (!showMods)
                    Hide();
                else
                    Show();
            }
        }

        public ModsContainer()
        {
            Children = new Drawable[]
            {
                iconsContainer = new FillFlowContainer<ModIcon>
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5,0),
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.TopCentre,
                    Text = @"/ UNRANKED /",
                    Font = @"Venera",
                    TextSize = 12,
                }
            };

            Mods.ValueChanged += mods =>
            {
                iconsContainer.Clear();
                foreach (Mod mod in mods)
                {
                    iconsContainer.Add(new ModIcon(mod)
                    {
                        AutoSizeAxes = Axes.Both,
                        Scale = new Vector2(0.7f),
                    });
                }
            };
        }
    }
}
