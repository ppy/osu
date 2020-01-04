// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Framework.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneSongProgress : OsuTestScene
    {
        private readonly SongProgress progress;
        private readonly TestSongProgressGraph graph;

        private readonly StopwatchClock clock;

        [Cached]
        private readonly GameplayClock gameplayClock;

        private readonly FramedClock framedClock;

        public TestSceneSongProgress()
        {
            clock = new StopwatchClock(true);

            gameplayClock = new GameplayClock(framedClock = new FramedClock(clock));

            Add(progress = new SongProgress
            {
                RelativeSizeAxes = Axes.X,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
            });

            Add(graph = new TestSongProgressGraph
            {
                RelativeSizeAxes = Axes.X,
                Height = 200,
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
            });

            AddWaitStep("wait some", 5);
            AddAssert("ensure not created", () => graph.CreationCount == 0);

            AddStep("display values", displayNewValues);
            AddWaitStep("wait some", 5);
            AddUntilStep("wait for creation count", () => graph.CreationCount == 1);

            AddStep("Toggle Bar", () => progress.AllowSeeking = !progress.AllowSeeking);
            AddWaitStep("wait some", 5);
            AddUntilStep("wait for creation count", () => graph.CreationCount == 1);

            AddStep("Toggle Bar", () => progress.AllowSeeking = !progress.AllowSeeking);
            AddWaitStep("wait some", 5);
            AddUntilStep("wait for creation count", () => graph.CreationCount == 1);
            AddRepeatStep("New Values", displayNewValues, 5);

            AddWaitStep("wait some", 5);
            AddAssert("ensure debounced", () => graph.CreationCount == 2);
        }

        private void displayNewValues()
        {
            List<HitObject> objects = new List<HitObject>();
            for (double i = 0; i < 5000; i += RNG.NextDouble() * 10 + i / 1000)
                objects.Add(new HitObject { StartTime = i });

            progress.Objects = objects;
            graph.Objects = objects;

            progress.RequestSeek = pos => clock.Seek(pos);
        }

        protected override void Update()
        {
            base.Update();
            framedClock.ProcessFrame();
        }

        private class TestSongProgressGraph : SongProgressGraph
        {
            public int CreationCount { get; private set; }

            protected override void RecreateGraph()
            {
                base.RecreateGraph();
                CreationCount++;
            }
        }
    }
}
