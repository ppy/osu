// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public class StableDirectorySelectScreen : DirectorySelectScreen
    {
        private readonly TaskCompletionSource<string> taskCompletionSource;

        protected override bool IsValidDirectory(DirectoryInfo info) => info?.GetFiles("osu!.*.cfg").Any() ?? false;

        protected override OsuSpriteText CreateHeader() => new OsuSpriteText
        {
            Text = "Please select stable location",
            Font = OsuFont.Default.With(size: 40)
        };

        public StableDirectorySelectScreen(TaskCompletionSource<string> taskCompletionSource)
        {
            this.taskCompletionSource = taskCompletionSource;
        }

        protected override void OnSelection(DirectoryInfo directory)
        { 
            taskCompletionSource.TrySetResult(directory.FullName);
            this.Exit();
        }

        public override bool OnBackButton()
        {
            taskCompletionSource.TrySetCanceled();
            return base.OnBackButton();
        }
    }
}
