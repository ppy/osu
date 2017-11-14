// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using osu.Framework;
using osu.Framework.Platform;
using osu.Game.IPC;

namespace osu.Desktop
{
    public static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            // Back up the cwd before DesktopGameHost changes it
            var cwd = Environment.CurrentDirectory;

            // TEMPORARY - Should be removed with .NET Core (removal of Squirrel)
            // See: https://github.com/dotnet/corefx/issues/19914#issuecomment-302327100
            var netHttp = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "System.Net.Http.dll");
            if (RuntimeInfo.IsMono && File.Exists(netHttp))
                File.Delete(netHttp);

            using (DesktopGameHost host = Host.GetSuitableHost(@"osu", true))
            {
                if (!host.IsPrimaryInstance)
                {
                    var importer = new BeatmapIPCChannel(host);
                    // Restore the cwd so relative paths given at the command line work correctly
                    Directory.SetCurrentDirectory(cwd);
                    foreach (var file in args)
                    {
                        Console.WriteLine(@"Importing {0}", file);
                        if (!importer.ImportAsync(Path.GetFullPath(file)).Wait(3000))
                            throw new TimeoutException(@"IPC took too long to send");
                    }
                }
                else
                {
                    switch (args.FirstOrDefault() ?? string.Empty)
                    {
                        case "--tests":
                            host.Run(new OsuTestBrowser());
                            break;
                        default:
                            host.Run(new OsuGameDesktop(args));
                            break;
                    }

                }
                return 0;
            }
        }
    }
}
