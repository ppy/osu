// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Utils;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneTimelineZoom : TimelineTestScene
    {
        public override Drawable CreateTestComponent() => Empty();

        [Test]
        public void TestVisibleRangeViaZoom()
        {
            double initialVisibleRange = 0;

            AddStep("reset zoom", () => TimelineArea.Timeline.Zoom = 100);
            AddStep("get initial range", () => initialVisibleRange = TimelineArea.Timeline.VisibleRange);

            AddStep("scale zoom", () => TimelineArea.Timeline.Zoom = 200);
            AddAssert("range halved", () => Precision.AlmostEquals(TimelineArea.Timeline.VisibleRange, initialVisibleRange / 2, 1));
            AddStep("descale zoom", () => TimelineArea.Timeline.Zoom = 50);
            AddAssert("range doubled", () => Precision.AlmostEquals(TimelineArea.Timeline.VisibleRange, initialVisibleRange * 2, 1));

            AddStep("restore zoom", () => TimelineArea.Timeline.Zoom = 100);
            AddAssert("range restored", () => Precision.AlmostEquals(TimelineArea.Timeline.VisibleRange, initialVisibleRange, 1));
        }

        [Test]
        public void TestVisibleRangeViaTimelineSize()
        {
            double initialVisibleRange = 0;

            AddStep("reset timeline size", () => TimelineArea.Timeline.Width = 1);
            AddStep("get initial range", () => initialVisibleRange = TimelineArea.Timeline.VisibleRange);

            AddStep("scale timeline size", () => TimelineArea.Timeline.Width = 2);
            AddAssert("range doubled", () => TimelineArea.Timeline.VisibleRange == initialVisibleRange * 2);
            AddStep("descale timeline size", () => TimelineArea.Timeline.Width = 0.5f);
            AddAssert("range halved", () => TimelineArea.Timeline.VisibleRange == initialVisibleRange / 2);

            AddStep("restore timeline size", () => TimelineArea.Timeline.Width = 1);
            AddAssert("range restored", () => TimelineArea.Timeline.VisibleRange == initialVisibleRange);
        }
    }
}
