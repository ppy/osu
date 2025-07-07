// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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

        public virtual ReleaseStream? FixedReleaseStream => null;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [Resolved]
        private OsuGameBase game { get; set; } = null!;

        [Resolved]
        protected INotificationOverlay Notifications { get; private set; } = null!;

        protected IBindable<ReleaseStream> ReleaseStream => releaseStream;

        private readonly Bindable<ReleaseStream> releaseStream = new Bindable<ReleaseStream>();

        private CancellationTokenSource updateCancellationSource = new CancellationTokenSource();

        protected override void LoadComplete()
        {
            base.LoadComplete();

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

            config.BindWith(OsuSetting.ReleaseStream, releaseStream);
            releaseStream.BindValueChanged(_ => CheckForUpdate());

            CheckForUpdate();
        }

        /// <summary>
        /// Immediately checks for any available update.
        /// </summary>
        public void CheckForUpdate()
        {
            _ = CheckForUpdateAsync();
        }

        /// <summary>
        /// Immediately checks for any available update.
        /// </summary>
        /// <returns><c>true</c> if any updates are available, <c>false</c> otherwise.</returns>
        public async Task<bool> CheckForUpdateAsync(CancellationToken cancellationToken = default) => await Task.Run(async () =>
        {
            if (!CanCheckForUpdate)
                return false;

            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Cancels the last update and closes any existing notifications as stale.
            using (var lastCts = Interlocked.Exchange(ref updateCancellationSource, cts))
                await lastCts.CancelAsync().ConfigureAwait(false);

            return await PerformUpdateCheck(cts.Token).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Performs an asynchronous check for application updates.
        /// </summary>
        /// <returns>Whether any update is waiting. May return true if an error occured (there is potentially an update available).</returns>
        protected virtual Task<bool> PerformUpdateCheck(CancellationToken cancellationToken) => Task.FromResult(false);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            updateCancellationSource.Cancel();
            updateCancellationSource.Dispose();
        }

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
                    changelog.ShowBuild(version);
                    return true;
                };
            }
        }

        public partial class UpdateDownloadProgressNotification : ProgressNotification
        {
            private readonly CancellationToken cancellationToken;

            public UpdateDownloadProgressNotification(CancellationToken cancellationToken)
            {
                this.cancellationToken = cancellationToken;
            }

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

            protected override void Update()
            {
                base.Update();

                if (cancellationToken.IsCancellationRequested)
                    FailDownload();
            }

            public void StartDownload()
            {
                State = ProgressNotificationState.Active;
                Progress = 0;
                Text = NotificationsStrings.DownloadingUpdate;
            }

            public void FailDownload()
            {
                State = ProgressNotificationState.Cancelled;
                Close(false);
            }

            protected override Notification CreateCompletionNotification() => new UpdateReadyNotification(cancellationToken)
            {
                Activated = () =>
                {
                    if (cancellationToken.IsCancellationRequested)
                        return true;

                    return CompletionClickAction?.Invoke() ?? true;
                }
            };
        }

        public partial class UpdateReadyNotification : ProgressCompletionNotification
        {
            private readonly CancellationToken cancellationToken;

            public UpdateReadyNotification(CancellationToken cancellationToken)
            {
                this.cancellationToken = cancellationToken;
                Text = NotificationsStrings.UpdateReadyToInstall;
            }

            protected override void Update()
            {
                base.Update();

                if (cancellationToken.IsCancellationRequested)
                    Close(false);
            }
        }

        public partial class UpdateAvailableNotification : SimpleNotification
        {
            private readonly CancellationToken cancellationToken;

            public UpdateAvailableNotification(CancellationToken cancellationToken)
            {
                this.cancellationToken = cancellationToken;
                Icon = FontAwesome.Solid.Download;
            }

            protected override void Update()
            {
                base.Update();

                if (cancellationToken.IsCancellationRequested)
                    Close(false);
            }
        }
    }
}
