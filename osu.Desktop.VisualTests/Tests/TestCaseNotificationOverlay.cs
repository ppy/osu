// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.MathUtils;
using osu.Game.Overlays;
using System.Linq;
using osu.Game.Overlays.Notifications;
using osu.Framework.Graphics.Containers;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseNotificationOverlay : TestCase
    {
        public override string Description => @"I handle full-screen notifications";

        private NotificationOverlay overlay;

        public override void Reset()
        {
            base.Reset();

            Content.Add(overlay = new NotificationOverlay());

            AddStep(@"Notification 1", () => overlay.ShowNotification(@"Notification 1"));
            AddStep(@"Notification 2", () => overlay.ShowNotification(@"Notification 2 (a bit longer)"));
            AddStep(@"Notification 3", () => overlay.ShowNotification(@"Another notification"));
        }
    }
}
