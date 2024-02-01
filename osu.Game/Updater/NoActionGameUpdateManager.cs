// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

namespace osu.Game.Updater
{
    /// <summary>
    /// An update manager that shows notifications if a newer release of osu! is detected.
    /// This is a case where updates are handled externally by a package manager or other means, so no action is performed on clicking the notification.
    /// </summary>
    public partial class NoActionGameUpdateManager : SimpleGameUpdateManager
    {
        protected override string DownloadMessage => "Check with your package manager / provider to bring osu! up-to-date!";

        // We intentionally ignore the user click.
        protected override void UpdateActionOnClick(GitHubRelease release) {}
    }
}
