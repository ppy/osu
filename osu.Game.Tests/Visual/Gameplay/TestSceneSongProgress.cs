// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Framework.Timing;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneSongProgress : OsuTestScene
    {
        private SongProgress progress;
        private TestSongProgressGraph graph;
        private readonly Container progressContainer;

        private readonly StopwatchClock clock;
        private readonly FramedClock framedClock;

        [Cached]
        private readonly GameplayClock gameplayClock;

        public TestSceneSongProgress()
        {
            clock = new StopwatchClock();
            gameplayClock = new GameplayClock(framedClock = new FramedClock(clock));

            Add(progressContainer = new Container
            {
                RelativeSizeAxes = Axes.X,
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                Height = 100,
                Y = -100,
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(1),
                }
            });
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

                progressContainer.Add(progress = new SongProgress
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
            AddUntilStep("wait for graph", () => graph.CreationCount == 1);
            AddStep("start", clock.Start);
            AddStep("allow seeking", () => progress.AllowSeeking.Value = true);
            AddStep("hide graph", () => progress.ShowGraph.Value = false);
            AddStep("disallow seeking", () => progress.AllowSeeking.Value = false);
            AddStep("allow seeking", () => progress.AllowSeeking.Value = true);
            AddStep("show graph", () => progress.ShowGraph.Value = true);
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
