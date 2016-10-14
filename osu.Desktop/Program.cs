//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using System.Linq;
using osu.Framework;
using osu.Framework.Desktop;
using osu.Framework.Platform;
using osu.Game;

namespace osu.Desktop
{
    public static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            BasicGameHost host = Host.GetSuitableHost(@"osu");
            BaseGame osuGame = new OsuGame();
            if (args.Length != 0 && args.All(File.Exists))
            {
                host.Load(osuGame);
                var beatmapIPC = new IpcChannel<OsuGame.ImportBeatmap>(host);
                foreach (var file in args)
                    beatmapIPC.SendMessage(new OsuGame.ImportBeatmap { Path = file }).Wait();
                Console.WriteLine(@"Sent file to running instance");
                return 0;
            }
            host.Add(osuGame);
            host.Run();
            return 0;
        }
    }
}
