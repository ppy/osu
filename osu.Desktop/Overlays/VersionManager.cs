using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using Squirrel;
using System.Reflection;

namespace osu.Desktop.Overlays
{
    public class VersionManager : OverlayContainer
    {
        private UpdateManager updateManager;
        private NotificationManager notification;

        [BackgroundDependencyLoader]
        private void load(NotificationManager notification)
        {
            this.notification = notification;

            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;

            var asm = Assembly.GetEntryAssembly().GetName();
            Add(new OsuSpriteText
            {
                Text = $@"osu!lazer v{asm.Version}"
            });

            updateChecker();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            State = Visibility.Visible;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            updateManager?.Dispose();
        }

        private async void updateChecker()
        {
            updateManager = await UpdateManager.GitHubUpdateManager(@"https://github.com/ppy/osu", @"osulazer", null, null, true);

            if (!updateManager.IsInstalledApp)
                return;

            var info = await updateManager.CheckForUpdate();
            if (info.ReleasesToApply.Count > 0)
            {
                ProgressNotification n = new UpdateProgressNotification
                {
                    Text = @"Downloading update..."
                };
                Schedule(() => notification.Post(n));
                Schedule(() => n.State = ProgressNotificationState.Active);
                await updateManager.DownloadReleases(info.ReleasesToApply, (int p) => Schedule(() => n.Progress = p / 100f));
                Schedule(() => n.Text = @"Installing update...");
                await updateManager.ApplyReleases(info, (int p) => Schedule(() => n.Progress = p / 100f));
                Schedule(() => n.State = ProgressNotificationState.Completed);

            }
            else
            {
                //check again every 30 minutes.
                Scheduler.AddDelayed(updateChecker, 60000 * 30);
            }
        }

        protected override void PopIn()
        {
        }

        protected override void PopOut()
        {
        }

        class UpdateProgressNotification : ProgressNotification
        {
            protected override Notification CreateCompletionNotification() => new ProgressCompletionNotification(this)
            {
                Text = @"Update ready to install. Click to restart!",
                Activated = () =>
                {
                    UpdateManager.RestartApp();
                    return true;
                }
            };
        }
    }
}
