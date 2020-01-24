// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneGraph : OsuTestScene
    {
        public TestSceneGraph()
        {
            BarGraph graph;

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

            AddStep("values from 1-10", () => graph.Values = Enumerable.Range(1, 10).Select(i => (float)i));
            AddStep("values from 1-100", () => graph.Values = Enumerable.Range(1, 100).Select(i => (float)i));
            AddStep("reversed values from 1-10", () => graph.Values = Enumerable.Range(1, 10).Reverse().Select(i => (float)i));
            AddStep("Bottom to top", () => graph.Direction = BarDirection.BottomToTop);
            AddStep("Top to bottom", () => graph.Direction = BarDirection.TopToBottom);
            AddStep("Left to right", () => graph.Direction = BarDirection.LeftToRight);
            AddStep("Right to left", () => graph.Direction = BarDirection.RightToLeft);
        }
    }
}
