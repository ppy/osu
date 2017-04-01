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
    internal class TestCaseNotificationManager : TestCase
    {
        public override string Description => @"I handle notifications";

        private NotificationManager manager;

        public override void Reset()
        {
            base.Reset();

            progressingNotifications.Clear();

            Content.Add(manager = new NotificationManager
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
            {
                Delay(80);
                Schedule(() => sendBarrage(remaining - 1));
            }
        }

        protected override void Update()
        {
            base.Update();

            progressingNotifications.RemoveAll(n => n.State == ProgressNotificationState.Completed);

            while (progressingNotifications.Count(n => n.State == ProgressNotificationState.Active) < 3)
            {
                var p = progressingNotifications.FirstOrDefault(n => n.IsLoaded && n.State == ProgressNotificationState.Queued);
                if (p == null)
                    break;

                p.State = ProgressNotificationState.Active;
            }

            foreach (var n in progressingNotifications.FindAll(n => n.State == ProgressNotificationState.Active))
            {
                if (n.Progress < 1)
                    n.Progress += (float)(Time.Elapsed / 2000) * RNG.NextSingle();
                else
                    n.State = ProgressNotificationState.Completed;
            }
        }

        private void sendProgress2()
        {
            var n = new ProgressNotification { Text = @"Downloading Haitai..." };
            manager.Post(n);
            progressingNotifications.Add(n);
        }

        private readonly List<ProgressNotification> progressingNotifications = new List<ProgressNotification>();

        private void sendProgress1()
        {
            var n = new ProgressNotification { Text = @"Uploading to BSS..." };
            manager.Post(n);
            progressingNotifications.Add(n);
        }

        private void sendNotification2()
        {
            manager.Post(new SimpleNotification { Text = @"You are amazing" });
        }

        private void sendNotification1()
        {
            manager.Post(new SimpleNotification { Text = @"Welcome to osu!. Enjoy your stay!" });
        }
    }
}
