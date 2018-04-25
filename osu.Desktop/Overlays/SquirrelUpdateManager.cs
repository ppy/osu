// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

#if NET_FRAMEWORK
using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Game;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using OpenTK;
using OpenTK.Graphics;
using Squirrel;

namespace osu.Desktop.Overlays
{
    public class SquirrelUpdateManager : Component
    {
        private UpdateManager updateManager;
        private NotificationOverlay notificationOverlay;

        public void PrepareUpdate()
        {
            // Squirrel returns execution to us after the update process is started, so it's safe to use Wait() here
            UpdateManager.RestartAppWhenExited().Wait();
        }

        [BackgroundDependencyLoader]
        private void load(NotificationOverlay notification, OsuGameBase game)
        {
            notificationOverlay = notification;

            if (game.IsDeployedBuild)
                Schedule(() => checkForUpdateAsync());
        }

        private async void checkForUpdateAsync(bool useDeltaPatching = true, UpdateProgressNotification notification = null)
        {
            //should we schedule a retry on completion of this check?
            bool scheduleRetry = true;

            try
            {
                if (updateManager == null) updateManager = await UpdateManager.GitHubUpdateManager(@"https://github.com/ppy/osu", @"osulazer", null, null, true);

                var info = await updateManager.CheckForUpdate(!useDeltaPatching);
                if (info.ReleasesToApply.Count == 0)
                    //no updates available. bail and retry later.
                    return;

                if (notification == null)
                {
                    notification = new UpdateProgressNotification(this) { State = ProgressNotificationState.Active };
                    Schedule(() => notificationOverlay.Post(notification));
                }

                notification.Progress = 0;
                notification.Text = @"Downloading update...";

                try
                {
                    await updateManager.DownloadReleases(info.ReleasesToApply, p => notification.Progress = p / 100f);

                    notification.Progress = 0;
                    notification.Text = @"Installing update...";

                    await updateManager.ApplyReleases(info, p => notification.Progress = p / 100f);

                    notification.State = ProgressNotificationState.Completed;
                }
                catch (Exception e)
                {
                    if (useDeltaPatching)
                    {
                        Logger.Error(e, @"delta patching failed!");

                        //could fail if deltas are unavailable for full update path (https://github.com/Squirrel/Squirrel.Windows/issues/959)
                        //try again without deltas.
                        checkForUpdateAsync(false, notification);
                        scheduleRetry = false;
                    }
                    else
                    {
                        Logger.Error(e, @"update failed!");
                    }
                }
            }
            catch (Exception)
            {
                // we'll ignore this and retry later. can be triggered by no internet connection or thread abortion.
            }
            finally
            {
                if (scheduleRetry)
                {
                    if (notification != null)
                        notification.State = ProgressNotificationState.Cancelled;

                    //check again in 30 minutes.
                    Scheduler.AddDelayed(() => checkForUpdateAsync(), 60000 * 30);
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            updateManager?.Dispose();
        }

        private class UpdateProgressNotification : ProgressNotification
        {
            private readonly SquirrelUpdateManager updateManager;
            private OsuGame game;

            public UpdateProgressNotification(SquirrelUpdateManager updateManager)
            {
                this.updateManager = updateManager;
            }

            protected override Notification CreateCompletionNotification()
            {
                return new ProgressCompletionNotification
                {
                    Text = @"Update ready to install. Click to restart!",
                    Activated = () =>
                    {
                        updateManager.PrepareUpdate();
                        game.GracefullyExit();
                        return true;
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, OsuGame game)
            {
                this.game = game;

                IconContent.AddRange(new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = ColourInfo.GradientVertical(colours.YellowDark, colours.Yellow)
                    },
                    new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Icon = FontAwesome.fa_upload,
                        Colour = Color4.White,
                        Size = new Vector2(20),
                    }
                });
            }
        }
    }
}
#endif
