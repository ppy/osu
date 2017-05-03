// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK;

namespace osu.Game.Screens.Play
{
    public class ModsContainer : Container
    {
        private readonly FillFlowContainer<ModIcon> iconsContainer;

        private bool showMods;
        public bool ShowMods
        {
            set
            {
                showMods = value;
                if (!showMods) Hide();
            }
            get { return ShowMods; }
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
        }

        public void Add(Mod mod)
        {
            iconsContainer.Add(new ModIcon(mod)
            {
                AutoSizeAxes = Axes.Both,
                Scale = new Vector2((float)0.7),
            });
        }
    }
}
