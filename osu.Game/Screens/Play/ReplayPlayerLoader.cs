// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.Play
{
    public class ReplayPlayerLoader : PlayerLoader
    {
        private readonly IReadOnlyList<Mod> mods;

        public ReplayPlayerLoader(Func<Player> player, IReadOnlyList<Mod> mods)
            : base(player)
        {
            this.mods = mods;
        }

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            dependencies = new DependencyContainer(parent);
            dependencies.Cache(new Bindable<IReadOnlyList<Mod>>(mods));

            return base.CreateChildDependencies(dependencies);
        }
    }
}
