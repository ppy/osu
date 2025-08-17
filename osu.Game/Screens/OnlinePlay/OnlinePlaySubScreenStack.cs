// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.OnlinePlay.Lounge;

namespace osu.Game.Screens.OnlinePlay
{
    public partial class OnlinePlaySubScreenStack : OsuScreenStack
    {
        private OsuScreenDependencies dependencies = null!;

        // Note - these bindables must be stored to fields of this component to be correctly unbound on disposal.
        private Bindable<WorkingBeatmap> beatmap = null!;
        private Bindable<RulesetInfo> ruleset = null!;
        private Bindable<IReadOnlyList<Mod>> mods = null!;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            // Bindables are leased by the OnlinePlayScreen, but pulled locally in order to not rely on screen load timings.
            // They will all be initially enabled while there is no screen in this stack.
            dependencies = new OsuScreenDependencies(true, base.CreateChildDependencies(parent))
            {
                Beatmap = { Disabled = false },
                Ruleset = { Disabled = false },
                Mods = { Disabled = false }
            };

            beatmap = dependencies.Beatmap;
            ruleset = dependencies.Ruleset;
            mods = dependencies.Mods;

            return dependencies;
        }

        protected override void ScreenChanged(IScreen prev, IScreen? next)
        {
            base.ScreenChanged(prev, next);

            if (next is not OsuScreen osuNext)
                throw new InvalidOperationException("There must always be an online play subscreen.");

            // See: OnlinePlayScreen.DisallowExternalBeatmapRulesetChanges.
            //
            // Bindable leases are held by the OnlinePlayScreen and NOT by the subscreens,
            // because PlayerLoader needs to resolve LeasedBindables to function correctly.
            //
            // An unfortunate consequence of this is we need to manually control bindable
            // enablement depending on what effect the subscreens want.
            //
            // This is a two-part process...

            // First, emulate the behaviour of DisallowExternalBeatmapRulesetChanges to disable toolbar buttons.
            beatmap.Disabled = osuNext.DisallowExternalBeatmapRulesetChanges;
            ruleset.Disabled = osuNext.DisallowExternalBeatmapRulesetChanges;
            mods.Disabled = osuNext.DisallowExternalBeatmapRulesetChanges;

            // Second, when an OsuScreen is exited with DisallowExternalBeatmapRulesetChanges=true, leased bindables
            // are normally returned which reverts the mod and ruleset bindables to their original states.
            //
            // The exact behaiour of the revert is awkward to emulate, but we particularly care about resetting mods
            // when returning to the lounge so that they don't stick around if the user then goes to create a new room.
            if (next is LoungeSubScreen)
                mods.Value = [];
        }
    }
}
