// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Updater
{
    /// <summary>
    /// An update manager that shows notifications if a newer release is detected.
    /// Installation is left up to the user.
    /// </summary>
    public partial class SimpleUpdateManager : UpdateManager
    {
        private readonly IUpdateChecker updateChecker;

        public SimpleUpdateManager(IUpdateChecker updateChecker)
        {
            this.updateChecker = updateChecker;
        }

        [Resolved]
        private GameHost host { get; set; }

        protected override async Task<bool> PerformUpdateCheck()
        {
            var updateInfo = await updateChecker.CheckForUpdates().ConfigureAwait(false);

            if (updateInfo is null)
                return false;

            Notifications.Post(new SimpleNotification
            {
                Text = $"A newer release of {updateInfo.Name} has been found ({updateInfo.CurrentVersion} → {updateInfo.NewVersion}).\n\n"
                       + "Click here to download the new version, which can be installed over the top of your existing installation",
                Icon = FontAwesome.Solid.Download,
                Activated = () =>
                {
                    host.OpenUrlExternally(updateInfo.DownloadUrl);
                    return true;
                }
            });

            return true;
        }
    }
}
