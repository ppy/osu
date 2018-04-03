// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Framework.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseSongProgress : OsuTestCase
    {
        private readonly SongProgress progress;
        private readonly SongProgressGraph graph;

        private readonly StopwatchClock clock;

        public TestCaseSongProgress()
        {
            clock = new StopwatchClock(true);

            Add(progress = new SongProgress
            {
                RelativeSizeAxes = Axes.X,
                AudioClock = new StopwatchClock(true),
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
            });

            Add(graph = new SongProgressGraph
            {
                RelativeSizeAxes = Axes.X,
                Height = 200,
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
            });

            AddStep("Toggle Bar", () => progress.AllowSeeking = !progress.AllowSeeking);
            AddWaitStep(5);
            AddStep("Toggle Bar", () => progress.AllowSeeking = !progress.AllowSeeking);
            AddWaitStep(2);
            AddRepeatStep("New Values", displayNewValues, 5);

            displayNewValues();
        }

        private void displayNewValues()
        {
            List<HitObject> objects = new List<HitObject>();
            for (double i = 0; i < 5000; i += RNG.NextDouble() * 10 + i / 1000)
                objects.Add(new HitObject { StartTime = i });

            progress.Objects = objects;
            graph.Objects = objects;

            progress.AudioClock = clock;
            progress.OnSeek = pos => clock.Seek(pos);
        }
    }
}
