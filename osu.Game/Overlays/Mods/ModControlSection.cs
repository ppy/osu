// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public class ModControlSection : Container
    {
        protected FillFlowContainer FlowContent;
        protected override Container<Drawable> Content => FlowContent;

        public readonly Mod Mod;

        public ModControlSection(Mod mod)
        {
            Mod = mod;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            FlowContent = new FillFlowContainer
            {
                Margin = new MarginPadding { Top = 30 },
                Spacing = new Vector2(0, 5),
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
            };

            AddRange(Mod.CreateSettingsControls());
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AddRangeInternal(new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = Mod.Name,
                    Font = OsuFont.GetFont(weight: FontWeight.Bold),
                    Colour = colours.Yellow,
                },
                FlowContent
            });
        }
    }
}
