// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using System.Linq;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseGraph : TestCase
    {
        public override string Description => "graph";

        private BarGraph graph;

        public override void Reset()
        {
            base.Reset();

            Children = new[]
            {
                graph = new BarGraph
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.5f),
                },
            };

            AddStep("values from 1-10", () => graph.Values = Enumerable.Range(1,10).Select(i => (float)i));
            AddStep("values from 1-100", () => graph.Values = Enumerable.Range(1, 100).Select(i => (float)i));
            AddStep("reversed values from 1-10", () => graph.Values = Enumerable.Range(1, 10).Reverse().Select(i => (float)i));
            AddStep("Bottom to top", () => graph.Direction = BarDirection.BottomToTop);
            AddStep("Top to bottom", () => graph.Direction = BarDirection.TopToBottom);
            AddStep("Left to right", () => graph.Direction = BarDirection.LeftToRight);
            AddStep("Right to left", () => graph.Direction = BarDirection.RightToLeft);
        }
    }
}