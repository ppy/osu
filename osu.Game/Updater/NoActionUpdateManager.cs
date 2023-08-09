// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Threading.Tasks;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Updater
{
    /// <summary>
    /// An update manager that shows notifications if a newer release is detected.
    /// This is a case where updates are handled externally by a package manager or other means, so no action is performed on clicking the notification.
    /// </summary>
    public partial class NoActionUpdateManager : UpdateManager
    {
        private readonly IUpdateChecker updateChecker;

        public NoActionUpdateManager(IUpdateChecker updateChecker)
        {
            this.updateChecker = updateChecker;
        }

        protected override async Task<bool> PerformUpdateCheck()
        {
            var updateInfo = await updateChecker.CheckForUpdates().ConfigureAwait(false);

            if (updateInfo == null)
                return false;

            Notifications.Post(new SimpleNotification
            {
                Text = $"A newer release of {updateInfo.Name} has been found ({updateInfo.CurrentVersion} → {updateInfo.NewVersion}).\n\n"
                       + "Check with your package manager / provider to bring osu! up-to-date!",
                Icon = FontAwesome.Solid.Download,
            });

            return true;
        }
    }
}
