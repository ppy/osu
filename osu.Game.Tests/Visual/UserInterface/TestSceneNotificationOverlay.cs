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
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneNotificationOverlay : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(NotificationSection),
            typeof(SimpleNotification),
            typeof(ProgressNotification),
            typeof(ProgressCompletionNotification),
            typeof(IHasCompletionTarget),
            typeof(Notification)
        };

        private NotificationOverlay notificationOverlay;

        private readonly List<ProgressNotification> progressingNotifications = new List<ProgressNotification>();

        private SpriteText displayedCount;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            progressingNotifications.Clear();

            Content.Children = new Drawable[]
            {
                notificationOverlay = new NotificationOverlay
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight
                },
                displayedCount = new OsuSpriteText()
            };

            notificationOverlay.UnreadCount.ValueChanged += count => { displayedCount.Text = $"displayed count: {count.NewValue}"; };
        });

        [Test]
        public void TestBasicFlow()
        {
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

            checkDisplayedCount(33);

            AddWaitStep("wait some", 10);

            checkProgressingCount(0);
        }

        [Test]
        public void TestImportantWhileClosed()
        {
            AddStep(@"simple #1", sendHelloNotification);

            AddAssert("Is visible", () => notificationOverlay.State.Value == Visibility.Visible);

            checkDisplayedCount(1);

            AddStep(@"progress #1", sendUploadProgress);
            AddStep(@"progress #2", sendDownloadProgress);

            checkProgressingCount(2);
            checkDisplayedCount(3);
        }

        [Test]
        public void TestUnimportantWhileClosed()
        {
            AddStep(@"background #1", sendBackgroundNotification);

            AddAssert("Is not visible", () => notificationOverlay.State.Value == Visibility.Hidden);

            checkDisplayedCount(1);

            AddStep(@"background progress #1", sendBackgroundUploadProgress);

            AddWaitStep("wait some", 5);

            checkProgressingCount(0);

            checkDisplayedCount(2);

            AddStep(@"simple #1", sendHelloNotification);

            checkDisplayedCount(3);
        }

        [Test]
        public void TestSpam()
        {
            setState(Visibility.Visible);
            AddRepeatStep("send barrage", sendBarrage, 10);
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

        private void checkDisplayedCount(int expected) =>
            AddAssert($"Displayed count is {expected}", () => notificationOverlay.UnreadCount.Value == expected);

        private void sendDownloadProgress()
        {
            var n = new ProgressNotification
            {
                Text = @"Downloading Haitai...",
                CompletionText = "Downloaded Haitai!",
            };
            notificationOverlay.Post(n);
            progressingNotifications.Add(n);
        }

        private void sendUploadProgress()
        {
            var n = new ProgressNotification
            {
                Text = @"Uploading to BSS...",
                CompletionText = "Uploaded to BSS!",
            };
            notificationOverlay.Post(n);
            progressingNotifications.Add(n);
        }

        private void sendBackgroundUploadProgress()
        {
            var n = new BackgroundProgressNotification
            {
                Text = @"Uploading to BSS...",
                CompletionText = "Uploaded to BSS!",
            };
            notificationOverlay.Post(n);
            progressingNotifications.Add(n);
        }

        private void setState(Visibility state) => AddStep(state.ToString(), () => notificationOverlay.State.Value = state);

        private void checkProgressingCount(int expected) => AddAssert($"progressing count is {expected}", () => progressingNotifications.Count == expected);

        private void sendBarrage()
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
        }

        private void sendAmazingNotification()
        {
            notificationOverlay.Post(new SimpleNotification { Text = @"You are amazing" });
        }

        private void sendHelloNotification()
        {
            notificationOverlay.Post(new SimpleNotification { Text = @"Welcome to osu!. Enjoy your stay!" });
        }

        private void sendBackgroundNotification()
        {
            notificationOverlay.Post(new BackgroundNotification { Text = @"Welcome to osu!. Enjoy your stay!" });
        }

        private void sendManyNotifications()
        {
            for (int i = 0; i < 10; i++)
                notificationOverlay.Post(new SimpleNotification { Text = @"Spam incoming!!" });
        }

        private class BackgroundNotification : SimpleNotification
        {
            public override bool IsImportant => false;
        }

        private class BackgroundProgressNotification : ProgressNotification
        {
            public override bool IsImportant => false;
        }
    }
}
