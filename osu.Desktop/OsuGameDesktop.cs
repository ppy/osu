// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game;
using System.Linq;
using System.Windows.Forms;
using osu.Framework.Platform;
using osu.Framework.Desktop.Platform;
using osu.Desktop.Overlays;
using System.Reflection;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Menu;

namespace osu.Desktop
{
    internal class OsuGameDesktop : OsuGame
    {
        private readonly VersionManager versionManager;

        public OsuGameDesktop(string[] args = null)
            : base(args)
        {
            versionManager = new VersionManager
            {
                Depth = int.MinValue,
                State = Visibility.Hidden
            };
        }

        public override Storage GetStorageForStableInstall()
        {
            try
            {
                return new StableStorage();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// A method of accessing an osu-stable install in a controlled fashion.
        /// </summary>
        private class StableStorage : DesktopStorage
        {
            protected override string LocateBasePath()
            {
                Func<string, bool> checkExists = p => Directory.Exists(Path.Combine(p, "Songs"));

                string stableInstallPath;

                try
                {
                    using (RegistryKey key = Registry.ClassesRoot.OpenSubKey("osu"))
                        stableInstallPath = key?.OpenSubKey(@"shell\open\command")?.GetValue(String.Empty).ToString().Split('"')[1].Replace("osu!.exe", "");

                    if (checkExists(stableInstallPath))
                        return stableInstallPath;
                }
                catch
                {
                }

                stableInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"osu!");
                if (checkExists(stableInstallPath))
                    return stableInstallPath;

                stableInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".osu");
                if (checkExists(stableInstallPath))
                    return stableInstallPath;

                return null;
            }

            public StableStorage()
                : base(string.Empty)
            {
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            LoadComponentAsync(versionManager, Add);
            ScreenChanged += s =>
            {
                if (!versionManager.IsPresent && s is Intro)
                    versionManager.State = Visibility.Visible;
            };
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);
            var desktopWindow = host.Window as DesktopGameWindow;
            if (desktopWindow != null)
            {
                desktopWindow.CursorState |= CursorState.Hidden;

                desktopWindow.Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream(GetType(), "lazer.ico"));
                desktopWindow.Title = Name;

                desktopWindow.DragEnter += dragEnter;
                desktopWindow.DragDrop += dragDrop;
            }
        }

        private void dragDrop(DragEventArgs e)
        {
            // this method will only be executed if e.Effect in dragEnter gets set to something other that None.
            var dropData = (object[])e.Data.GetData(DataFormats.FileDrop);
            var filePaths = dropData.Select(f => f.ToString()).ToArray();

            if (filePaths.All(f => Path.GetExtension(f) == @".osz"))
                Task.Run(() => BeatmapManager.Import(filePaths));
            else if (filePaths.All(f => Path.GetExtension(f) == @".osr"))
                Task.Run(() =>
                {
                    var score = ScoreStore.ReadReplayFile(filePaths.First());
                    Schedule(() => LoadScore(score));
                });
        }

        private static readonly string[] allowed_extensions = { @".osz", @".osr" };

        private void dragEnter(DragEventArgs e)
        {
            // dragDrop will only be executed if e.Effect gets set to something other that None in this method.
            bool isFile = e.Data.GetDataPresent(DataFormats.FileDrop);
            if (isFile)
            {
                var paths = ((object[])e.Data.GetData(DataFormats.FileDrop)).Select(f => f.ToString()).ToArray();
                e.Effect = allowed_extensions.Any(ext => paths.All(p => p.EndsWith(ext))) ? DragDropEffects.Copy : DragDropEffects.None;
            }
        }
    }
}
