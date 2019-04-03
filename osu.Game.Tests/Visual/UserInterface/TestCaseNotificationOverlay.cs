// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.MathUtils;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestCaseNotificationOverlay : OsuTestCase
    {
        private readonly NotificationOverlay manager;
        private readonly List<ProgressNotification> progressingNotifications = new List<ProgressNotification>();

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(NotificationSection),
            typeof(SimpleNotification),
            typeof(ProgressNotification),
            typeof(ProgressCompletionNotification),
            typeof(IHasCompletionTarget),
            typeof(Notification)
        };

        public TestCaseNotificationOverlay()
        {
            progressingNotifications.Clear();

            Content.Add(manager = new NotificationOverlay
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight
            });

            SpriteText displayedCount = new SpriteText();

            Content.Add(displayedCount);

            void setState(Visibility state) => AddStep(state.ToString(), () => manager.State = state);
            void checkProgressingCount(int expected) => AddAssert($"progressing count is {expected}", () => progressingNotifications.Count == expected);

            manager.UnreadCount.ValueChanged += count => { displayedCount.Text = $"displayed count: {count.NewValue}"; };

            setState(Visibility.Visible);
            AddStep(@"simple #1", sendHelloNotification);
            AddStep(@"simple #2", sendAmazingNotification);
            AddStep(@"progress #1", sendUploadProgress);
            AddStep(@"progress #2", sendDownloadProgress);

            checkProgressingCount(2);

            setState(Visibility.Hidden);

            AddRepeatStep(@"add many simple", sendManyNotifications, 3);
            AddWaitStep("wait some", 5);

            checkProgressingCount(0);

            AddStep(@"progress #3", sendUploadProgress);

            checkProgressingCount(1);

            AddAssert("Displayed count is 33", () => manager.UnreadCount.Value == 33);

            AddWaitStep("wait some", 10);

            checkProgressingCount(0);

            setState(Visibility.Visible);

            //AddStep(@"barrage", () => sendBarrage());
        }

        private void sendBarrage(int remaining = 10)
        {
            switch (RNG.Next(0, 4))
            {
                case 0:
                    sendHelloNotification();
                    break;
                case 1:
                    sendAmazingNotification();
                    break;
                case 2:
                    sendUploadProgress();
                    break;
                case 3:
                    sendDownloadProgress();
                    break;
            }

            if (remaining > 0)
                Scheduler.AddDelayed(() => sendBarrage(remaining - 1), 80);
        }

        protected override void Update()
        {
            base.Update();

            progressingNotifications.RemoveAll(n => n.State == ProgressNotificationState.Completed);

            if (progressingNotifications.Count(n => n.State == ProgressNotificationState.Active) < 3)
            {
                var p = progressingNotifications.Find(n => n.State == ProgressNotificationState.Queued);
                if (p != null)
                    p.State = ProgressNotificationState.Active;
            }

            foreach (var n in progressingNotifications.FindAll(n => n.State == ProgressNotificationState.Active))
            {
                if (n.Progress < 1)
                    n.Progress += (float)(Time.Elapsed / 400) * RNG.NextSingle();
                else
                    n.State = ProgressNotificationState.Completed;
            }
        }

        private void sendDownloadProgress()
        {
            var n = new ProgressNotification
            {
                Text = @"Downloading Haitai...",
                CompletionText = "Downloaded Haitai!",
            };
            manager.Post(n);
            progressingNotifications.Add(n);
        }

        private void sendUploadProgress()
        {
            var n = new ProgressNotification
            {
                Text = @"Uploading to BSS...",
                CompletionText = "Uploaded to BSS!",
            };
            manager.Post(n);
            progressingNotifications.Add(n);
        }

        private void sendAmazingNotification()
        {
            manager.Post(new SimpleNotification { Text = @"You are amazing" });
        }

        private void sendHelloNotification()
        {
            manager.Post(new SimpleNotification { Text = @"Welcome to osu!. Enjoy your stay!" });
        }

        private void sendManyNotifications()
        {
            for (int i = 0; i < 10; i++)
                manager.Post(new SimpleNotification { Text = @"Spam incoming!!" });
        }
    }
}
