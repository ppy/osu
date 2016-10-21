//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using System.Linq;
using osu.Framework;
using osu.Framework.Desktop;
using osu.Framework.Desktop.Platform;
using osu.Framework.Platform;
using osu.Game;
using osu.Game.IPC;

namespace osu.Desktop
{
    public static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            DesktopGameHost host = Host.GetSuitableHost(@"osu", true);

            if (!host.IsPrimaryInstance)
            {
                var importer = new BeatmapImporter(host);
                
                foreach (var file in args)
                    if (importer.Import(file).Wait(1000))
                        throw new TimeoutException(@"IPC took too long to send");
                Console.WriteLine(@"Sent import requests to running instance");
            }
            else
            {
                BaseGame osu = new OsuGame(args);
                host.Add(osu);
                host.Run();
            }

            return 0;
        }
    }
}
