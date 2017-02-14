﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using osu.Framework.Platform;
using osu.Framework.Desktop.Platform;
using osu.Game.Database;
using osu.Desktop.Overlays;
using System.Reflection;
using System.Drawing;

namespace osu.Desktop
{
    class OsuGameDesktop : OsuGame
    {
        public OsuGameDesktop(string[] args = null)
            : base(args)
        {

        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            (new VersionManager()).Preload(this, Add);
        }

        public override void SetHost(BasicGameHost host)
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
            ImportBeatmaps(filePaths);
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
