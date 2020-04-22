// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Overlays;
using osu.Game.Overlays.Dashboard;
using osu.Game.Overlays.Dashboard.Friends;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneDashboardOverlay : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DashboardOverlay),
            typeof(DashboardOverlayHeader),
            typeof(FriendDisplay)
        };

        protected override bool UseOnlineAPI => true;

        private readonly DashboardOverlay overlay;

        public TestSceneDashboardOverlay()
        {
            Add(overlay = new DashboardOverlay());
        }

        [Test]
        public void TestShow()
        {
            AddStep("Show", overlay.Show);
        }

        [Test]
        public void TestHide()
        {
            AddStep("Hide", overlay.Hide);
        }
    }
}
