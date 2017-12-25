// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Tests.Visual
{
    public class TestCaseNotificationOverlay : OsuTestCase
    {
        private readonly NotificationOverlay manager;
        private readonly List<ProgressNotification> progressingNotifications = new List<ProgressNotification>();

        public TestCaseNotificationOverlay()
        {
            progressingNotifications.Clear();

            Content.Add(manager = new NotificationOverlay
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight
            });

            AddStep(@"toggle", manager.ToggleVisibility);
            AddStep(@"simple #1", sendHelloNotification);
            AddStep(@"simple #2", sendAmazingNotification);
            AddStep(@"progress #1", sendUploadProgress);
            AddStep(@"progress #2", sendDownloadProgress);
            AddStep(@"barrage", () => sendBarrage());
        }

        private void sendBarrage(int remaining = 100)
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
                var p = progressingNotifications.FirstOrDefault(n => n.IsAlive && n.State == ProgressNotificationState.Queued);
                if (p != null)
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
    }
}
