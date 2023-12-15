// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Localisation;
using osu.Framework.Screens;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public partial class StableDirectorySelectScreen : DirectorySelectScreen
    {
        private readonly TaskCompletionSource<string> taskCompletionSource;

        protected override OverlayActivation InitialOverlayActivationMode => OverlayActivation.Disabled;

        protected override bool IsValidDirectory(DirectoryInfo? info) =>
            // A full stable installation will have a configuration file present.
            // This is the best case scenario, as it may contain a custom beatmap directory we need to traverse to.
            info?.GetFiles("osu!.*.cfg").Any() == true ||
            // The user may only have their songs or skins folders left.
            // We still want to allow them to import based on this.
            info?.GetDirectories("Songs").Any() == true ||
            info?.GetDirectories("Skins").Any() == true ||
            // The user may have traverse *inside* their songs or skins folders.
            shouldUseParentDirectory(info);

        public override LocalisableString HeaderText => "Please select your osu!stable install location";

        public StableDirectorySelectScreen(TaskCompletionSource<string> taskCompletionSource)
        {
            this.taskCompletionSource = taskCompletionSource;
        }

        protected override void OnSelection(DirectoryInfo directory)
        {
            taskCompletionSource.TrySetResult(shouldUseParentDirectory(directory) ? directory.Parent!.FullName : directory.FullName);
            this.Exit();
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            taskCompletionSource.TrySetCanceled();
            return base.OnExiting(e);
        }

        private bool shouldUseParentDirectory(DirectoryInfo? info)
            => info?.Parent != null && (info.Name == "Songs" || info.Name == "Skins");
    }
}
