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

        protected override bool IsValidDirectory(DirectoryInfo? info) => info?.GetFiles("osu!.*.cfg").Any() ?? false;

        public override LocalisableString HeaderText => "Please select your osu!stable install location";

        public StableDirectorySelectScreen(TaskCompletionSource<string> taskCompletionSource)
        {
            this.taskCompletionSource = taskCompletionSource;
        }

        protected override void OnSelection(DirectoryInfo directory)
        {
            taskCompletionSource.TrySetResult(directory.FullName);
            this.Exit();
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            taskCompletionSource.TrySetCanceled();
            return base.OnExiting(e);
        }
    }
}
