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
using System.IO;
using System.Threading.Tasks;
using osu.Game.Screens.Menu;

namespace osu.Desktop
{
    internal class OsuGameDesktop : OsuGame
    {
        private readonly VersionManager versionManager;

        public OsuGameDesktop(string[] args = null)
            : base(args)
        {
            versionManager = new VersionManager { Depth = int.MinValue };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            LoadComponentAsync(versionManager);
            ScreenChanged += s =>
            {
                if (!versionManager.IsAlive && s is Intro)
                    Add(versionManager);
            };
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);
            var desktopWindow = host.Window as DesktopGameWindow;
            if (desktopWindow != null)
            {
                desktopWindow.CursorState = CursorState.Hidden;

                desktopWindow.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
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
                Task.Run(() => BeatmapDatabase.Import(filePaths));
            else if (filePaths.All(f => Path.GetExtension(f) == @".osr"))
                Task.Run(() =>
                {
                    var score = ScoreDatabase.ReadReplayFile(filePaths.First());
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
