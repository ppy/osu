// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public class GameplayIsolationContainer : Container
    {
        [Cached]
        private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        [Cached]
        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        [Cached]
        private readonly Bindable<IReadOnlyList<Mod>> mods = new Bindable<IReadOnlyList<Mod>>();

        public GameplayIsolationContainer(WorkingBeatmap beatmap, RulesetInfo ruleset, IReadOnlyList<Mod> mods)
        {
            this.beatmap.Value = beatmap;
            this.ruleset.Value = ruleset;
            this.mods.Value = mods;

            beatmap.LoadTrack();
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.CacheAs(ruleset.BeginLease(false));
            dependencies.CacheAs(beatmap.BeginLease(false));
            dependencies.CacheAs(mods.BeginLease(false));
            return dependencies;
        }
    }
}
