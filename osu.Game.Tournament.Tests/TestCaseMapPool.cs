// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Tournament.Screens.MapPool;

namespace osu.Game.Tournament.Tests
{
    public class TestCaseMapPool : LadderTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(MapPoolScreen)
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new MapPoolScreen { Width = 0.7f });
        }
    }
}
