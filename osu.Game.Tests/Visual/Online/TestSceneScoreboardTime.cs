// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Overlays.BeatmapSet.Scores;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneScoreboardTime : OsuTestScene
    {
        private StopwatchClock stopwatch;

        [Test]
        public void TestVariousUnits()
        {
            AddStep("create various scoreboard times", () => Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Clock = new FramedClock(stopwatch = new StopwatchClock()), // prevent time from naturally elapsing.
                Direction = FillDirection.Vertical,
                ChildrenEnumerable = testCases.Select(dateTime => new ScoreboardTime(dateTime, 24).With(time => time.Anchor = time.Origin = Anchor.TopCentre))
            });

            AddStep("start stopwatch", () => stopwatch.Start());
        }

        private static IEnumerable<DateTimeOffset> testCases => new[]
        {
            DateTimeOffset.Now,
            DateTimeOffset.Now.AddSeconds(-1),
            DateTimeOffset.Now.AddSeconds(-25),
            DateTimeOffset.Now.AddSeconds(-59),
            DateTimeOffset.Now.AddMinutes(-1),
            DateTimeOffset.Now.AddMinutes(-25),
            DateTimeOffset.Now.AddMinutes(-59),
            DateTimeOffset.Now.AddHours(-1),
            DateTimeOffset.Now.AddHours(-13),
            DateTimeOffset.Now.AddHours(-23),
            DateTimeOffset.Now.AddDays(-1),
            DateTimeOffset.Now.AddDays(-6),
            DateTimeOffset.Now.AddDays(-16),
            DateTimeOffset.Now.AddMonths(-1),
            DateTimeOffset.Now.AddMonths(-11),
            DateTimeOffset.Now.AddYears(-1),
            DateTimeOffset.Now.AddYears(-5)
        };
    }
}
