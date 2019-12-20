// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public class ModControlSection : CompositeDrawable
    {
        protected FillFlowContainer FlowContent;

        public readonly Mod Mod;

        public ModControlSection(Mod mod, IEnumerable<Drawable> modControls)
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
                ChildrenEnumerable = modControls
            };
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
