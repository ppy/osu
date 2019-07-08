// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;

namespace osu.Game.Screens.Play
{
    public class ReplayPlayerLoader : PlayerLoader
    {
        private readonly Bindable<IReadOnlyList<Mod>> mods;

        public ReplayPlayerLoader(Score score, IReadOnlyList<Mod> mods)
            : base(() => new ReplayPlayer(score))
        {
            this.mods = new Bindable<IReadOnlyList<Mod>>(mods);
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.Cache(mods);

            // Overwrite the global mods here for use in the mod hud.
            Mods.Value = mods.Value;

            return dependencies;
        }
    }
}
