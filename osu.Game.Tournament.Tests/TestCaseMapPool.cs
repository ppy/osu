// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Tournament.Screens.MapPool;

namespace osu.Game.Tournament.Tests
{
    public class TestCaseMapPool : LadderTestCase
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            var round = Ladder.Groupings.FirstOrDefault(g => g.Name == "Finals");

            if (round != null)
                Add(new MapPoolScreen(round));
        }
    }
}
