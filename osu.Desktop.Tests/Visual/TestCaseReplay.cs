// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Screens.Play;
using System.Linq;

namespace osu.Desktop.Tests.Visual
{
    internal class TestCaseReplay : TestCasePlayer
    {
        public override string Description => @"Testing replay playback.";

        protected override Player CreatePlayer(WorkingBeatmap beatmap, Ruleset ruleset)
        {
            beatmap.Mods.Value = beatmap.Mods.Value.Concat(new[] { ruleset.GetAutoplayMod() });
            return base.CreatePlayer(beatmap, ruleset);
        }
    }
}
