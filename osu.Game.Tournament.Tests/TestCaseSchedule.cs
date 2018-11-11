// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.Screens.Schedule;

namespace osu.Game.Tournament.Tests
{
    public class TestCaseSchedule : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ScheduleScreen)
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new ScheduleScreen());
        }
    }
}
