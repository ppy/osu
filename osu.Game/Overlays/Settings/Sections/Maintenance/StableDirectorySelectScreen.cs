// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game.Database;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public partial class StableDirectorySelectScreen : DirectorySelectScreen
    {
        private readonly TaskCompletionSource<string> taskCompletionSource;

        [Resolved]
        private LegacyImportManager legacyImportManager { get; set; } = null!;

        protected override OverlayActivation InitialOverlayActivationMode => OverlayActivation.Disabled;

        protected override bool IsValidDirectory(DirectoryInfo? info) => legacyImportManager.IsUsableForStableImport(info, out _);

        public override LocalisableString HeaderText => "Please select your osu!stable install location";

        public StableDirectorySelectScreen(TaskCompletionSource<string> taskCompletionSource)
        {
            this.taskCompletionSource = taskCompletionSource;
        }

        protected override void OnSelection(DirectoryInfo directory)
        {
            if (!legacyImportManager.IsUsableForStableImport(directory, out var stableRoot))
                throw new InvalidOperationException($@"{nameof(OnSelection)} was called on an invalid directory. This should never happen.");

            taskCompletionSource.TrySetResult(stableRoot.FullName);
            this.Exit();
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            taskCompletionSource.TrySetCanceled();
            return base.OnExiting(e);
        }
    }
}
