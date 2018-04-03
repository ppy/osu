// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using System.Linq;
using osu.Framework;
using osu.Framework.Platform;
using osu.Game.IPC;
#if NET_FRAMEWORK
using System.Runtime;
#endif

namespace osu.Desktop
{
    public static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            // required to initialise native SQLite libraries on some platforms.

            if (!RuntimeInfo.IsMono)
                useMulticoreJit();

            // Back up the cwd before DesktopGameHost changes it
            var cwd = Environment.CurrentDirectory;

            using (DesktopGameHost host = Host.GetSuitableHost(@"osu", true))
            {
                if (!host.IsPrimaryInstance)
                {
                    var importer = new ArchiveImportIPCChannel(host);
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
                        default:
                            host.Run(new OsuGameDesktop(args));
                            break;
                    }
                }

                return 0;
            }
        }

        private static void useMulticoreJit()
        {
#if NET_FRAMEWORK
            var directory = Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Profiles"));
            ProfileOptimization.SetProfileRoot(directory.FullName);
            ProfileOptimization.StartProfile("Startup.Profile");
#endif
        }
    }
}
