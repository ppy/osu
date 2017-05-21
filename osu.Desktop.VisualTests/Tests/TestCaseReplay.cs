// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Play;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseReplay : TestCasePlayer
    {
        public override string Description => @"Testing replay playback.";

        protected override Player CreatePlayer(WorkingBeatmap beatmap)
        {
            beatmap.Mods.Value = new Mod[] { new OsuModAutoplay() };

            return base.CreatePlayer(beatmap);
        }
    }
}
