// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using osu.Desktop.Overlays;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Game;
using osuTK.Input;
using osu.Desktop.Updater;
using osu.Framework;
using osu.Framework.Screens;
using osu.Game.Screens.Menu;

namespace osu.Desktop
{
    internal abstract class OsuGameDesktop : OsuGame
    {
        private readonly bool noVersionOverlay;
        private VersionManager versionManager;

        protected OsuGameDesktop(string[] args = null)
            : base(args)
        {
            noVersionOverlay = args?.Any(a => a == "--no-version-overlay") ?? false;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (!noVersionOverlay)
            {
                LoadComponentAsync(versionManager = new VersionManager { Depth = int.MinValue }, v =>
                {
                    Add(v);
                    v.State = Visibility.Visible;
                });

                if (RuntimeInfo.OS == RuntimeInfo.Platform.Windows)
                    Add(new SquirrelUpdateManager());
                else
                    Add(new SimpleUpdateManager());
            }
        }

        protected override void ScreenChanged(IScreen lastScreen, IScreen newScreen)
        {
            base.ScreenChanged(lastScreen, newScreen);

            switch (newScreen)
            {
                case Intro _:
                case MainMenu _:
                    if (versionManager != null)
                        versionManager.State = Visibility.Visible;
                    break;

                default:
                    if (versionManager != null)
                        versionManager.State = Visibility.Hidden;
                    break;
            }
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);

            if (host.Window is DesktopGameWindow desktopWindow)
            {
                desktopWindow.CursorState |= CursorState.Hidden;

                desktopWindow.SetIconFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(OsuGameDesktop), "lazer.ico"));
                desktopWindow.Title = Name;

                desktopWindow.FileDrop += fileDrop;
            }
        }

        private void fileDrop(object sender, FileDropEventArgs e)
        {
            var filePaths = e.FileNames;

            var firstExtension = Path.GetExtension(filePaths.First());

            if (filePaths.Any(f => Path.GetExtension(f) != firstExtension)) return;

            Task.Factory.StartNew(() => Import(filePaths), TaskCreationOptions.LongRunning);
        }
    }
}
