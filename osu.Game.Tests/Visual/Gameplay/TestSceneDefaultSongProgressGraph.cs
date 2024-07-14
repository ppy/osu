// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public partial class TestSceneDefaultSongProgressGraph : OsuTestScene
    {
        private TestSongProgressGraph graph;

        [SetUpSteps]
        public void SetupSteps()
        {
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

        private void displayRandomValues()
        {
            var objects = new List<HitObject>();
            for (double i = 0; i < 5000; i += RNG.NextDouble() * 10 + i / 1000)
                objects.Add(new HitObject { StartTime = i });

            graph.Objects = objects;
        }

        private partial class TestSongProgressGraph : DefaultSongProgressGraph
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
