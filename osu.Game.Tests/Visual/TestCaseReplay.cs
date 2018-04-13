﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    [Description("Player instantiated with a replay.")]
    public class TestCaseReplay : TestCasePlayer
    {
        protected override Player CreatePlayer(WorkingBeatmap beatmap, Ruleset ruleset)
        {
            // We create a dummy RulesetContainer just to get the replay - we don't want to use mods here
            // to simulate setting a replay rather than having the replay already set for us
            beatmap.Mods.Value = beatmap.Mods.Value.Concat(new[] { ruleset.GetAutoplayMod() });
            var dummyRulesetContainer = ruleset.CreateRulesetContainerWith(beatmap, beatmap.BeatmapInfo.Ruleset.Equals(ruleset.RulesetInfo));

            // We have the replay
            var replay = dummyRulesetContainer.Replay;

            // Reset the mods
            beatmap.Mods.Value = beatmap.Mods.Value.Where(m => !(m is ModAutoplay));

            return new ReplayPlayer(replay)
            {
                InitialBeatmap = beatmap
            };
        }
    }
}
