// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

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
        protected NotificationOverlay Notifications { get; private set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Schedule(() => Task.Run(CheckForUpdateAsync));

            var version = game.Version;

            var lastVersion = config.Get<string>(OsuSetting.Version);

            if (game.IsDeployedBuild && version != lastVersion)
            {
                // only show a notification if we've previously saved a version to the config file (ie. not the first run).
                if (!string.IsNullOrEmpty(lastVersion))
                    Notifications.Post(new UpdateCompleteNotification(version));
            }

            // debug / local compilations will reset to a non-release string.
            // can be useful to check when an install has transitioned between release and otherwise (see OsuConfigManager's migrations).
            config.Set(OsuSetting.Version, version);
        }

        private readonly object updateTaskLock = new object();

        private Task updateCheckTask;

        public async Task CheckForUpdateAsync()
        {
            if (!CanCheckForUpdate)
                return;

            Task waitTask;

            lock (updateTaskLock)
                waitTask = (updateCheckTask ??= PerformUpdateCheck());

            await waitTask;

            lock (updateTaskLock)
                updateCheckTask = null;
        }

        protected virtual Task PerformUpdateCheck() => Task.CompletedTask;

        private class UpdateCompleteNotification : SimpleNotification
        {
            private readonly string version;

            public UpdateCompleteNotification(string version)
            {
                this.version = version;
                Text = $"You are now running osu!lazer {version}.\nClick to see what's new!";
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, ChangelogOverlay changelog, NotificationOverlay notificationOverlay)
            {
                Icon = FontAwesome.Solid.CheckSquare;
                IconBackgound.Colour = colours.BlueDark;

                Activated = delegate
                {
                    notificationOverlay.Hide();
                    changelog.ShowBuild(OsuGameBase.CLIENT_STREAM_NAME, version);
                    return true;
                };
            }
        }
    }
}
