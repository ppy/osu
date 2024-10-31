// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays.Mods
{
    public partial class MiniModSection : CompositeDrawable
    {
        public readonly ModType ModType;

        private readonly IReadOnlyList<ModState> availableMods = Array.Empty<ModState>();
        private readonly HashSet<Mod>? rootSet;

        private OsuSpriteText sectionHeader = null!;
        private FillFlowContainer modFlowContainer = null!;

        public MiniModSection(ModType modType, HashSet<Mod>? rootSet)
        {
            ModType = modType;
            this.rootSet = rootSet;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    sectionHeader = new OsuSpriteText
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = ModType.Humanize(LetterCasing.Title),
                        Colour = colours.ForModType(ModType),
                        Font = OsuFont.TorusAlternate.With(size: 16, weight: FontWeight.SemiBold),
                    },
                    modFlowContainer = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Margin = new MarginPadding
                        {
                            Left = 5,
                        }
                    }
                }
            };

            if (rootSet != null)
            {
                modFlowContainer.ChildrenEnumerable = rootSet.Where(m => m.Type == ModType)
                                                             .Select(m => new ModPresetRow(m, rootSet, ModRowMode.Add));
            }
        }

        private void updateState()
        {
            Alpha = availableMods.All(mod => !mod.Visible) ? 0 : 1;
        }
    }
}
