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
        private readonly IReadOnlyList<Mod> mods;

        public ReplayPlayerLoader(Score score)
            : base(() => new ReplayPlayer(score))
        {
            mods = score.ScoreInfo.Mods;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            // Overwrite the global mods here for use in the mod hud.
            Mods.Value = mods;

            return dependencies;
        }
    }
}
