﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Win32;
using osu.Desktop.Overlays;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Game;
using OpenTK.Input;

namespace osu.Desktop
{
    internal class OsuGameDesktop : OsuGame
    {
        public OsuGameDesktop(string[] args = null)
            : base(args)
        {
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

            LoadComponentAsync(new VersionManager { Depth = int.MinValue }, v =>
            {
                Add(v);
                v.State = Visibility.Visible;
            });
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

                desktopWindow.FileDrop += fileDrop;
            }
        }

        private void fileDrop(object sender, FileDropEventArgs e)
        {
            var filePaths = new [] { e.FileName };

            if (filePaths.All(f => Path.GetExtension(f) == @".osz"))
                Task.Factory.StartNew(() => BeatmapManager.Import(filePaths), TaskCreationOptions.LongRunning);
            else if (filePaths.All(f => Path.GetExtension(f) == @".osr"))
                Task.Run(() =>
                {
                    var score = ScoreStore.ReadReplayFile(filePaths.First());
                    Schedule(() => LoadScore(score));
                });
        }

        private static readonly string[] allowed_extensions = { @".osz", @".osr" };
    }
}
