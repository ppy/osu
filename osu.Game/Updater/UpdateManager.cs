// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Reflection;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Updater
{
    /// <summary>
    /// An update manager which only shows notifications after an update completes.
    /// </summary>
    public partial class UpdateManager : CompositeDrawable
    {
        /// <summary>
        /// Whether this UpdateManager should be or is capable of checking for updates.
        /// </summary>
        public bool CanCheckForUpdate => game.IsDeployedBuild &&
                                         // only implementations will actually check for updates.
                                         GetType() != typeof(UpdateManager);

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [Resolved]
        private OsuGameBase game { get; set; } = null!;

        [Resolved]
        protected INotificationOverlay Notifications { get; private set; } = null!;

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

                if (RuntimeInfo.EntryAssembly.GetCustomAttribute<OfficialBuildAttribute>() == null)
                    Notifications.Post(new SimpleNotification { Text = NotificationsStrings.NotOfficialBuild });
            }

            // debug / local compilations will reset to a non-release string.
            // can be useful to check when an install has transitioned between release and otherwise (see OsuConfigManager's migrations).
            config.SetValue(OsuSetting.Version, version);
        }

        private readonly object updateTaskLock = new object();

        private Task<bool>? updateCheckTask;

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

        private partial class UpdateCompleteNotification : SimpleNotification
        {
            private readonly string version;

            public UpdateCompleteNotification(string version)
            {
                this.version = version;
                Text = NotificationsStrings.GameVersionAfterUpdate(version);
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, ChangelogOverlay changelog, INotificationOverlay notificationOverlay)
            {
                Icon = FontAwesome.Solid.CheckSquare;
                IconContent.Colour = colours.BlueDark;

                Activated = delegate
                {
                    notificationOverlay.Hide();
                    changelog.ShowBuild(OsuGameBase.CLIENT_STREAM_NAME, version);
                    return true;
                };
            }
        }

        public partial class UpdateApplicationCompleteNotification : ProgressCompletionNotification
        {
            public UpdateApplicationCompleteNotification()
            {
                Text = NotificationsStrings.UpdateReadyToInstall;
            }
        }

        public partial class UpdateProgressNotification : ProgressNotification
        {
            protected override Notification CreateCompletionNotification() => new UpdateApplicationCompleteNotification
            {
                Activated = CompletionClickAction
            };

            [BackgroundDependencyLoader]
            private void load()
            {
                IconContent.AddRange(new Drawable[]
                {
                    new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Icon = FontAwesome.Solid.Download,
                        Size = new Vector2(34),
                        Colour = OsuColour.Gray(0.2f),
                        Depth = float.MaxValue,
                    }
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                StartDownload();
            }

            public override void Close(bool runFlingAnimation)
            {
                // cancelling updates is not currently supported by the underlying updater.
                // only allow dismissing for now.

                switch (State)
                {
                    case ProgressNotificationState.Cancelled:
                    case ProgressNotificationState.Completed:
                        base.Close(runFlingAnimation);
                        break;
                }
            }

            public void StartDownload()
            {
                State = ProgressNotificationState.Active;
                Progress = 0;
                Text = NotificationsStrings.DownloadingUpdate;
            }

            public void StartInstall()
            {
                Progress = 0;
                Text = NotificationsStrings.InstallingUpdate;
            }

            public void FailDownload()
            {
                State = ProgressNotificationState.Cancelled;
                Close(false);
            }
        }
    }
}
