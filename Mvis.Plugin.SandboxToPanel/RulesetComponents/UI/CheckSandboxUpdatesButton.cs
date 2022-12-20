using Mvis.Plugin.SandboxToPanel.RulesetComponents.Online;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Game.Overlays.Settings;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.UI
{
    public partial class CheckSandboxUpdatesButton : CompositeDrawable
    {
        private GetLatestReleaseRequest request;
        private readonly SettingsButton settingsButton;

        [Resolved]
        private GameHost host { get; set; }
        
        private string latestVersion;
        private bool updateAvailable;

        public CheckSandboxUpdatesButton()
        {
            Width = 300;
            AutoSizeAxes = Axes.Y;
            InternalChildren = new Drawable[]
            {
                settingsButton = new SettingsButton
                {
                    Text = "Check for updates",
                    Action = clickAction
                }
            };
        }

        private void clickAction()
        {
            if (updateAvailable)
            {
                host.OpenUrlExternally($"https://github.com/EVAST9919/lazer-sandbox/releases/tag/{latestVersion}");
                return;
            }

            checkUpdates();
        }

        private void checkUpdates()
        {
            cancelRequest();

            request = new GetLatestReleaseRequest();
            request.Finished += () => Schedule(() =>
            {
                latestVersion = request.ResponseObject.TagName;
                updateAvailable = latestVersion != SandboxRuleset.VERSION;

                settingsButton.Text = updateAvailable ? $"New version is available, click to download!" : "You are up-to-date!";

                if (updateAvailable)
                {
                    settingsButton.Enabled.Value = true;
                }
                else
                {
                    Scheduler.AddDelayed(() =>
                    {
                        settingsButton.Enabled.Value = true;
                        settingsButton.Text = "Check for updates";
                    }, 5000);
                }
            });

            request.Failed += (_) => Schedule(() =>
            {
                settingsButton.Text = "Update check failed, try again later!";

                Scheduler.AddDelayed(() =>
                {
                    settingsButton.Enabled.Value = true;
                    settingsButton.Text = "Check for updates";
                }, 5000);
            });

            settingsButton.Enabled.Value = false;
            settingsButton.Text = "Checking for updates...";
            request.PerformAsync();
        }

        private void cancelRequest()
        {
            Scheduler.CancelDelayedTasks();
            request?.Abort();
        }

        protected override void Dispose(bool isDisposing)
        {
            cancelRequest();
            base.Dispose(isDisposing);
        }
    }
}
