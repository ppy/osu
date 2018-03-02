// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Toolbar;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseToolbar : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ToolbarButton),
            typeof(ToolbarModeSelector),
            typeof(ToolbarModeButton),
            typeof(ToolbarNotificationButton),
        };

        public TestCaseToolbar()
        {
            var toolbar = new Toolbar { State = Visibility.Visible };

            Add(toolbar);

            var notificationButton = toolbar.Children.OfType<FillFlowContainer>().Last().Children.OfType<ToolbarNotificationButton>().First();

            void setNotifications(int count) => AddStep($"set notification count to {count}", () => notificationButton.NotificationCount.Value = count);

            setNotifications(1);
            setNotifications(2);
            setNotifications(3);
            setNotifications(0);
            setNotifications(144);
        }
    }
}
