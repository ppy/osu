// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Framework.Graphics;
using OpenTK.Graphics;
using osu.Game.Graphics;
using osu.Framework.Allocation;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Play
{
    public class ModsContainer : Container
    {
        private readonly FillFlowContainer<ModIcon> iconsContainer;

        private bool showMods;
        public bool ShowMods
        {
            get { return showMods; }
            set { showMods = value; }
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
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.TopCentre,
                    Text = @"/UNRANKED/",
                    Font = @"Venera",
                    TextSize = 15,
                }
            };
        }

        public void Add(Mod mod)
        {
            iconsContainer.Add(new ModIcon
            {
                AutoSizeAxes = Axes.Both,
                Icon = mod.Icon,
                Colour = selectColour(mod),
                IconSize = 60,
            });
        }

        private Color4 selectColour(Mod mod)
        {
            switch (mod.Type)
            {
                case ModType.DifficultyIncrease:
                    return OsuColour.FromHex(@"ffcc22");
                case ModType.DifficultyReduction:
                    return OsuColour.FromHex(@"88b300");
                case ModType.Special:
                    return OsuColour.FromHex(@"66ccff");

                default: return Color4.White;
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (ShowMods)
                Show();
            else
                Hide();
        }
    }
}
