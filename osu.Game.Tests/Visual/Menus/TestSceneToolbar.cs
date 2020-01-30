// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Toolbar;

namespace osu.Game.Tests.Visual.Menus
{
    [TestFixture]
    public class TestSceneToolbar : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ToolbarButton),
            typeof(ToolbarRulesetSelector),
            typeof(ToolbarRulesetTabButton),
            typeof(ToolbarNotificationButton),
        };

        public TestSceneToolbar()
        {
            var toolbar = new Toolbar { State = { Value = Visibility.Visible } };
            ToolbarNotificationButton notificationButton = null;

            AddStep("create toolbar", () =>
            {
                Add(toolbar);
                notificationButton = toolbar.Children.OfType<FillFlowContainer>().Last().Children.OfType<ToolbarNotificationButton>().First();
            });

            void setNotifications(int count) => AddStep($"set notification count to {count}", () => notificationButton.NotificationCount.Value = count);

            setNotifications(1);
            setNotifications(2);
            setNotifications(3);
            setNotifications(0);
            setNotifications(144);
        }
    }
}
