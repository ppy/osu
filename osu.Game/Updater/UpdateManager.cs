// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osuTK;

namespace osu.Game.Updater
{
    /// <summary>
    /// A base with provides a common foundation for any UpdateManagers
    /// </summary>
    public abstract partial class UpdateManager : CompositeDrawable
    {
        /// <summary>
        /// Whether this UpdateManager should be or is capable of checking for updates.
        /// </summary>
        public bool CanCheckForUpdate => game.IsDeployedBuild &&
                                         // only implementations will actually check for updates.
                                         GetType() != typeof(UpdateManager);

        [Resolved]
        private OsuGameBase game { get; set; } = null!;

        [Resolved]
        protected INotificationOverlay Notifications { get; private set; } = null!;

        private readonly object updateTaskLock = new object();

        private Task<bool>? updateCheckTask;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Schedule(() => Task.Run(CheckForUpdateAsync));
        }

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

        public partial class UpdateApplicationCompleteNotification : ProgressCompletionNotification
        {
            public UpdateApplicationCompleteNotification()
            {
                Text = @"Update ready to install. Click to restart!";
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
                Text = @"Downloading update...";
            }

            public void StartInstall()
            {
                Progress = 0;
                Text = @"Installing update...";
            }

            public void FailDownload()
            {
                State = ProgressNotificationState.Cancelled;
                Close(false);
            }
        }
    }
}
