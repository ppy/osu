// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Updater
{
    /// <summary>
    /// An update manager which only shows notifications after an update completes.
    /// </summary>
    public class UpdateManager : CompositeDrawable
    {
        /// <summary>
        /// Whether this UpdateManager should be or is capable of checking for updates.
        /// </summary>
        public bool CanCheckForUpdate => game.IsDeployedBuild &&
                                         // only implementations will actually check for updates.
                                         GetType() != typeof(UpdateManager);

        [Resolved]
        private OsuConfigManager config { get; set; }

        [Resolved]
        private OsuGameBase game { get; set; }

        [Resolved]
        protected INotificationOverlay Notifications { get; private set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Schedule(() => Task.Run(CheckForUpdateAsync));

            string version = game.Version;

            string lastVersion = config.Get<string>(OsuSetting.Version);

            if (game.IsDeployedBuild && version != lastVersion)
            {
                // only show a notification if we've previously saved a version to the config file (ie. not the first run).
                if (!string.IsNullOrEmpty(lastVersion))
                    Notifications.Post(new UpdateCompleteNotification(version));
            }

            // debug / local compilations will reset to a non-release string.
            // can be useful to check when an install has transitioned between release and otherwise (see OsuConfigManager's migrations).
            config.SetValue(OsuSetting.Version, version);
        }

        private readonly object updateTaskLock = new object();

        private Task<bool> updateCheckTask;

        public async Task<bool> CheckForUpdateAsync()
        {
            if (!CanCheckForUpdate)
                return false;

            Task<bool> waitTask;

            lock (updateTaskLock)
                waitTask = (updateCheckTask ??= PerformUpdateCheck());

            bool hasUpdates = await waitTask.ConfigureAwait(false);

            lock (updateTaskLock)
                updateCheckTask = null;

            return hasUpdates;
        }

        /// <summary>
        /// Performs an asynchronous check for application updates.
        /// </summary>
        /// <returns>Whether any update is waiting. May return true if an error occured (there is potentially an update available).</returns>
        protected virtual Task<bool> PerformUpdateCheck() => Task.FromResult(false);

        public virtual Task PrepareUpdateAsync() => Task.Run(() => { });

        /// <summary>
        /// Shows a notification when osu was launched with a new version.
        /// </summary>
        private class UpdateCompleteNotification : SimpleNotification
        {
            private readonly string version;

            public UpdateCompleteNotification(string version)
            {
                this.version = version;
                Text = $"You are now running osu! {version}.\nClick to see what's new!";
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, ChangelogOverlay changelog, INotificationOverlay notificationOverlay)
            {
                Icon = FontAwesome.Solid.CheckSquare;
                IconBackground.Colour = colours.BlueDark;

                Activated = delegate
                {
                    notificationOverlay.Hide();
                    changelog.ShowBuild(OsuGameBase.CLIENT_STREAM_NAME, version);
                    return true;
                };
            }
        }

        /// <summary>
        /// Shows a notification when the updated process has finished and the game is ready to be restarted.
        /// </summary>
        protected class ProgressCompleteNotification : ProgressCompletionNotification
        {
            [Resolved]
            private OsuGame game { get; set; }

            public ProgressCompleteNotification(UpdateManager updateManager)
            {
                Text = @"Update ready to install. Click to restart!";

                Activated = () =>
                {
                    updateManager.PrepareUpdateAsync()
                                 .ContinueWith(_ => updateManager.Schedule(() => game?.AttemptExit()));
                    return true;
                };
            }
        }

        protected class UpdateProgressNotification : ProgressNotification
        {
            private readonly UpdateManager updateManager;

            public UpdateProgressNotification(UpdateManager updateManager)
            {
                this.updateManager = updateManager;
            }

            protected override Notification CreateCompletionNotification()
            {
                return new ProgressCompleteNotification(updateManager);
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
    }
}
