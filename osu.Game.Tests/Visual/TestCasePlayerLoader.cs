// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    public class TestCasePlayerLoader : OsuTestCase
    {
        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            AddStep("load dummy beatmap", () => Add(new PlayerLoader(new Player
            {
                InitialBeatmap = new DummyWorkingBeatmap(game),
                AllowPause = false,
                AllowLeadIn = false,
                AllowResults = false,
            })));
        }
    }
}
