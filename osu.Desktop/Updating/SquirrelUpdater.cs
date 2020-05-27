// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osuTK;
using osuTK.Graphics;
using Squirrel;
using Logger = osu.Framework.Logging.Logger;
using LogLevel = Splat.LogLevel;

namespace osu.Desktop.Updating
{
    public class SquirrelUpdater : Game.Updating.Updater
    {
        private UpdateManager updateManager;

        private static readonly Logger logger = Logger.GetLogger("updater");

        [Resolved]
        private NotificationOverlay notifications { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Splat.Locator.CurrentMutable.Register(() => new SquirrelLogger(), typeof(Splat.ILogger));
        }

        public override async Task<bool> CheckAndPrepareAsync()
        {
            try
            {
                updateManager ??= await UpdateManager.GitHubUpdateManager(@"https://github.com/ppy/osu", @"osulazer", null, null, true);

                if (await internalCheckAndPrepareAsync())
                    return true;
            }
            catch
            {
                // we'll ignore this and retry later. can be triggered by no internet connection or thread abortion.
            }
            finally
            {
                //check again in 30 minutes.
                Scheduler.AddDelayed(async () => await CheckAndPrepareAsync(), 60000 * 30);
            }

            return false;
        }

        private async Task<bool> internalCheckAndPrepareAsync(bool useDeltaPatching = true, UpdateProgressNotification notification = null)
        {
            var info = await updateManager.CheckForUpdate(!useDeltaPatching);
            if (info.ReleasesToApply.Count == 0)
                //no updates available. bail and retry later.
                return false;

            if (notification == null)
            {
                notification = new UpdateProgressNotification(this) { State = ProgressNotificationState.Active };
                notifications.Post(notification);
            }

            try
            {
                notification.Progress = 0;
                notification.Text = @"Downloading update...";

                await updateManager.DownloadReleases(info.ReleasesToApply, p => notification.Progress = p / 100f);

                notification.Progress = 0;
                notification.Text = @"Installing update...";

                await updateManager.ApplyReleases(info, p => notification.Progress = p / 100f);

                notification.State = ProgressNotificationState.Completed;
                return true;
            }
            catch (Exception e)
            {
                if (useDeltaPatching)
                {
                    logger.Add(@"Delta patching failed, attempting full download.");

                    //could fail if deltas are unavailable for full update path (https://github.com/Squirrel/Squirrel.Windows/issues/959)
                    //try again without deltas.
                    return await internalCheckAndPrepareAsync(false, notification);
                }

                notification.State = ProgressNotificationState.Cancelled;
                Logger.Error(e, @"Failed to update.");
                return false;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            updateManager?.Dispose();
        }

        private class UpdateProgressNotification : ProgressNotification
        {
            private readonly SquirrelUpdater updater;
            private OsuGame game;

            public UpdateProgressNotification(SquirrelUpdater updater)
            {
                this.updater = updater;
            }

            protected override Notification CreateCompletionNotification()
            {
                return new ProgressCompletionNotification
                {
                    Text = @"Update ready to install. Click to restart!",
                    Activated = () =>
                    {
                        UpdateManager.RestartAppWhenExited()
                                     .ContinueWith(_ => updater.Schedule(() => game.GracefullyExit()));
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
                        Icon = FontAwesome.Solid.Upload,
                        Colour = Color4.White,
                        Size = new Vector2(20),
                    }
                });
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
