// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Game;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osuTK;
using osuTK.Graphics;
using Squirrel;
using LogLevel = Splat.LogLevel;

namespace osu.Desktop.Updater
{
    public class SquirrelUpdateManager : osu.Game.Updater.UpdateManager
    {
        private UpdateManager updateManager;
        private NotificationOverlay notificationOverlay;

        public Task PrepareUpdateAsync() => UpdateManager.RestartAppWhenExited();

        private static readonly Logger logger = Logger.GetLogger("updater");

        /// <summary>
        /// Whether an update has been downloaded but not yet applied.
        /// </summary>
        private bool updatePending;

        [BackgroundDependencyLoader]
        private void load(NotificationOverlay notification)
        {
            notificationOverlay = notification;

            Splat.Locator.CurrentMutable.Register(() => new SquirrelLogger(), typeof(Splat.ILogger));
        }

        protected override async Task<bool> PerformUpdateCheck() => await checkForUpdateAsync().ConfigureAwait(false);

        private async Task<bool> checkForUpdateAsync(bool useDeltaPatching = true, UpdateProgressNotification notification = null)
        {
            // should we schedule a retry on completion of this check?
            bool scheduleRecheck = true;

            try
            {
                updateManager ??= await UpdateManager.GitHubUpdateManager(@"https://github.com/ppy/osu", @"osulazer", null, null, true).ConfigureAwait(false);

                var info = await updateManager.CheckForUpdate(!useDeltaPatching).ConfigureAwait(false);

                if (info.ReleasesToApply.Count == 0)
                {
                    if (updatePending)
                    {
                        // the user may have dismissed the completion notice, so show it again.
                        notificationOverlay.Post(new UpdateCompleteNotification(this));
                        return true;
                    }

                    // no updates available. bail and retry later.
                    return false;
                }

                scheduleRecheck = false;

                if (notification == null)
                {
                    notification = new UpdateProgressNotification(this) { State = ProgressNotificationState.Active };
                    Schedule(() => notificationOverlay.Post(notification));
                }

                notification.Progress = 0;
                notification.Text = @"Downloading update...";

                try
                {
                    await updateManager.DownloadReleases(info.ReleasesToApply, p => notification.Progress = p / 100f).ConfigureAwait(false);

                    notification.Progress = 0;
                    notification.Text = @"Installing update...";

                    await updateManager.ApplyReleases(info, p => notification.Progress = p / 100f).ConfigureAwait(false);

                    notification.State = ProgressNotificationState.Completed;
                    updatePending = true;
                }
                catch (Exception e)
                {
                    if (useDeltaPatching)
                    {
                        logger.Add(@"delta patching failed; will attempt full download!");

                        // could fail if deltas are unavailable for full update path (https://github.com/Squirrel/Squirrel.Windows/issues/959)
                        // try again without deltas.
                        await checkForUpdateAsync(false, notification).ConfigureAwait(false);
                    }
                    else
                    {
                        notification.State = ProgressNotificationState.Cancelled;
                        Logger.Error(e, @"update failed!");
                    }
                }
            }
            catch (Exception)
            {
                // we'll ignore this and retry later. can be triggered by no internet connection or thread abortion.
                scheduleRecheck = true;
            }
            finally
            {
                if (scheduleRecheck)
                {
                    // check again in 30 minutes.
                    Scheduler.AddDelayed(() => Task.Run(async () => await checkForUpdateAsync().ConfigureAwait(false)), 60000 * 30);
                }
            }

            return true;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            updateManager?.Dispose();
        }

        private class UpdateCompleteNotification : ProgressCompletionNotification
        {
            [Resolved]
            private OsuGame game { get; set; }

            public UpdateCompleteNotification(SquirrelUpdateManager updateManager)
            {
                Text = @"Update ready to install. Click to restart!";

                Activated = () =>
                {
                    updateManager.PrepareUpdateAsync()
                                 .ContinueWith(_ => updateManager.Schedule(() => game?.GracefullyExit()));
                    return true;
                };
            }
        }

        private class UpdateProgressNotification : ProgressNotification
        {
            private readonly SquirrelUpdateManager updateManager;

            public UpdateProgressNotification(SquirrelUpdateManager updateManager)
            {
                this.updateManager = updateManager;
            }

            protected override Notification CreateCompletionNotification()
            {
                return new UpdateCompleteNotification(updateManager);
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
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
                        Icon = FontAwesome.Solid.Upload,
                        Colour = Color4.White,
                        Size = new Vector2(20),
                    }
                });
            }

            public override void Close()
            {
                // cancelling updates is not currently supported by the underlying updater.
                // only allow dismissing for now.

                switch (State)
                {
                    case ProgressNotificationState.Cancelled:
                        base.Close();
                        break;
                }
            }
        }

        private class SquirrelLogger : Splat.ILogger, IDisposable
        {
            public LogLevel Level { get; set; } = LogLevel.Info;

            public void Write(string message, LogLevel logLevel)
            {
                if (logLevel < Level)
                    return;

                logger.Add(message);
            }

            public void Dispose()
            {
            }
        }
    }
}
