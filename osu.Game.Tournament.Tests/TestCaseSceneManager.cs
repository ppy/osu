// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.Screens;

namespace osu.Game.Tournament.Tests
{
    public class TestCaseSceneManager : OsuTestCase
    {
        [BackgroundDependencyLoader]
        private void load(Storage storage)
        {
            Add(new TournamentSceneManager());
        }
    }
}
