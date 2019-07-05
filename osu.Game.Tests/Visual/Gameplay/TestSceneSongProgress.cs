// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneSongProgress : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(SongProgressBar),
        };

        private SongProgress progress;
        private TestSongProgressGraph graph;

        private readonly StopwatchClock clock;
        private readonly FramedClock framedClock;

        [Cached]
        private readonly GameplayClock gameplayClock;

        public TestSceneSongProgress()
        {
            clock = new StopwatchClock();
            gameplayClock = new GameplayClock(framedClock = new FramedClock(clock));
        }

        [SetUpSteps]
        public void SetupSteps()
        {
            AddStep("add new song progress", () =>
            {
                if (progress != null)
                {
                    progress.Expire();
                    progress = null;
                }

                Add(progress = new SongProgress
                {
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                });
            });

            AddStep("add new big graph", () =>
            {
                if (graph != null)
                {
                    graph.Expire();
                    graph = null;
                }

                Add(graph = new TestSongProgressGraph
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 200,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                });
            });

            AddStep("reset clock", clock.Reset);
        }

        [Test]
        public void TestGraphRecreation()
        {
            AddAssert("ensure not created", () => graph.CreationCount == 0);
            AddStep("display values", displayRandomValues);
            AddUntilStep("wait for creation count", () => graph.CreationCount == 1);
            AddRepeatStep("new values", displayRandomValues, 5);
            AddWaitStep("wait some", 5);
            AddAssert("ensure recreation debounced", () => graph.CreationCount == 2);
        }

        [Test]
        public void TestDisplay()
        {
            AddStep("display max values", displayMaxValues);
            AddStep("start", clock.Start);
            AddStep("show bar", () => progress.AllowSeeking = true);
            AddStep("hide graph", () => progress.CollapseGraph.Value = true);
            AddStep("hide Bar", () => progress.AllowSeeking = false);
            AddStep("show bar", () => progress.AllowSeeking = true);
            AddStep("show graph", () => progress.CollapseGraph.Value = false);
            AddStep("stop", clock.Stop);
        }

        private void displayRandomValues()
        {
            var objects = new List<HitObject>();
            for (double i = 0; i < 5000; i += RNG.NextDouble() * 10 + i / 1000)
                objects.Add(new HitObject { StartTime = i });

            replaceObjects(objects);
        }

        private void displayMaxValues()
        {
            var objects = new List<HitObject>();
            for (double i = 0; i < 5000; i++)
                objects.Add(new HitObject { StartTime = i });

            replaceObjects(objects);
        }

        private void replaceObjects(List<HitObject> objects)
        {
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
