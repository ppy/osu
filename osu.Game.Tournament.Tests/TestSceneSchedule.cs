// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.Screens.Schedule;

namespace osu.Game.Tournament.Tests
{
    public class TestSceneSchedule : OsuTestScene
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
