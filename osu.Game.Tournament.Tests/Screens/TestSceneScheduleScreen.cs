// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Screens.Schedule;

namespace osu.Game.Tournament.Tests.Screens
{
    public class TestSceneScheduleScreen : TournamentTestScene
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new TourneyVideo("main") { RelativeSizeAxes = Axes.Both });
            Add(new ScheduleScreen());
        }
    }
}
