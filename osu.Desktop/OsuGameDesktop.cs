// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game;
using System.Linq;
using System.Windows.Forms;
using osu.Framework.Platform;
using osu.Framework.Desktop.Platform;
using osu.Desktop.Overlays;
using System.Reflection;
using System.Drawing;
using osu.Game.Screens.Menu;

namespace osu.Desktop
{
    class OsuGameDesktop : OsuGame
    {
        private VersionManager versionManager;

        public override bool IsDeployedBuild => versionManager.IsDeployedBuild;

        public OsuGameDesktop(string[] args = null)
            : base(args)
        {
            versionManager = new VersionManager { Depth = int.MinValue };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            versionManager.LoadAsync(this);
            ModeChanged += m =>
            {
                if (!versionManager.IsAlive && m is Intro)
                    Add(versionManager);
            };
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);
            var desktopWindow = host.Window as DesktopGameWindow;
            if (desktopWindow != null)
            {
                desktopWindow.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
                desktopWindow.Title = @"osu!lazer";

                desktopWindow.DragEnter += dragEnter;
                desktopWindow.DragDrop += dragDrop;
            }
        }

        private void dragDrop(DragEventArgs e)
        {
            // this method will only be executed if e.Effect in dragEnter gets set to something other that None.
            var dropData = e.Data.GetData(DataFormats.FileDrop) as object[];
            var filePaths = dropData.Select(f => f.ToString()).ToArray();
            ImportBeatmapsAsync(filePaths);
        }

        private void dragEnter(DragEventArgs e)
        {
            // dragDrop will only be executed if e.Effect gets set to something other that None in this method.
            bool isFile = e.Data.GetDataPresent(DataFormats.FileDrop);
            if (isFile)
            {
                var paths = (e.Data.GetData(DataFormats.FileDrop) as object[]).Select(f => f.ToString()).ToArray();
                if (paths.Any(p => !p.EndsWith(".osz")))
                    e.Effect = DragDropEffects.None;
                else
                    e.Effect = DragDropEffects.Copy;
            }
        }
    }
}
