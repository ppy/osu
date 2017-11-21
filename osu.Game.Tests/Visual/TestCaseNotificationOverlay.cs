// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.MathUtils;
using osu.Game.Notifications;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    internal class TestCaseNotificationOverlay : OsuTestCase
    {
        public override string Description => @"I handle notifications";

        private readonly NotificationOverlay manager;

        public TestCaseNotificationOverlay()
        {
            progressingNotifications.Clear();

            Content.Add(manager = new NotificationOverlay
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            });

            AddToggleStep(@"show", state => manager.State = state ? Visibility.Visible : Visibility.Hidden);

            AddStep(@"simple #1", sendNotification1);
            AddStep(@"simple #2", sendNotification2);
            AddStep(@"progress #1", sendProgress1);
            AddStep(@"progress #2", sendProgress2);
            AddStep(@"barrage", () => sendBarrage());
        }

        private void sendBarrage(int remaining = 100)
        {
            switch (RNG.Next(0, 4))
            {
                case 0:
                    sendNotification1();
                    break;
                case 1:
                    sendNotification2();
                    break;
                case 2:
                    sendProgress1();
                    break;
                case 3:
                    sendProgress2();
                    break;
            }

            if (remaining > 0)
                Scheduler.AddDelayed(() => sendBarrage(remaining - 1), 80);
        }

        protected override void Update()
        {
            base.Update();

            progressingNotifications.RemoveAll(n => n.ProgressNotification.State == ProgressNotificationState.Completed);

            if (progressingNotifications.Count(n => n.ProgressNotification.State == ProgressNotificationState.Active) < 3)
            {
                var p = progressingNotifications.FirstOrDefault(n => n.IsAlive && n.ProgressNotification.State == ProgressNotificationState.Queued);
                if (p != null)
                    p.ProgressNotification.State = ProgressNotificationState.Active;
            }

            foreach (var n in progressingNotifications.FindAll(n => n.ProgressNotification.State == ProgressNotificationState.Active))
            {
                if (n.ProgressNotification.Progress < 1)
                    n.ProgressNotification.Progress += (float)(Time.Elapsed / 2000) * RNG.NextSingle();
                else
                    n.ProgressNotification.State = ProgressNotificationState.Completed;
            }
        }

        private void sendProgress2()
        {
            var n = new ProgressNotificationContainer(new ProgressNotification(@"Downloading Haitai..."));
            manager.Post(n);
            progressingNotifications.Add(n);
        }

        private readonly List<ProgressNotificationContainer> progressingNotifications = new List<ProgressNotificationContainer>();

        private void sendProgress1()
        {
            var n = new ProgressNotificationContainer(new ProgressNotification(@"Uploading to BSS..."));
            manager.Post(n);
            progressingNotifications.Add(n);
        }

        private void sendNotification2()
        {
            manager.Post(new SimpleNotificationContainer(@"You are amazing" ));
        }

        private void sendNotification1()
        {
            manager.Post(new SimpleNotificationContainer (@"Welcome to osu!. Enjoy your stay!" ));
        }
    }
}
