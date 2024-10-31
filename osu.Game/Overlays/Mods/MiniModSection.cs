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
        private readonly HashSet<Mod>? saveSet;

        public MiniModSection(ModType modType, HashSet<Mod>? rootSet, HashSet<Mod>? saveSet)
        {
            ModType = modType;
            this.rootSet = rootSet;
            this.saveSet = saveSet;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            if (rootSet != null && rootSet.Count(m => m.Type == ModType) != 0)
            {
                InternalChild = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = ModType.Humanize(LetterCasing.Title),
                            Colour = colours.ForModType(ModType),
                            Font = OsuFont.TorusAlternate.With(size: 16, weight: FontWeight.SemiBold),
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Margin = new MarginPadding
                            {
                                Left = 5,
                            },
                            ChildrenEnumerable = rootSet.Where(m => m.Type == ModType)
                                                        .Select(m => new ModPresetRow(m, rootSet, ModRowMode.Add, saveSet))
                        }
                    }
                };
            }
        }

        private void updateState()
        {
            Alpha = availableMods.All(mod => !mod.Visible) ? 0 : 1;
        }
    }
}
