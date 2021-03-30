// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Game.Tournament.IO;
using osu.Game.Tournament.IPC;

namespace osu.Game.Tournament.Tests.NonVisual
{
    [TestFixture]
    public class IPCLocationTest
    {
        [Test]
        public void CheckIPCLocation()
        {
            // don't use clean run because files are being written before osu! launches.
            using (HeadlessGameHost host = new HeadlessGameHost(nameof(CheckIPCLocation)))
            {
                string basePath = Path.Combine(RuntimeInfo.StartupDirectory, "headless", nameof(CheckIPCLocation));

                // Set up a fake IPC client for the IPC Storage to switch to.
                string testCeDir = Path.Combine(basePath, "stable-ce");
                Directory.CreateDirectory(testCeDir);

                string ipcFile = Path.Combine(testCeDir, "ipc.txt");
                File.WriteAllText(ipcFile, string.Empty);

                try
                {
                    var osu = loadOsu(host);
                    TournamentStorage storage = (TournamentStorage)osu.Dependencies.Get<Storage>();
                    FileBasedIPC ipc = (FileBasedIPC)osu.Dependencies.Get<MatchIPCInfo>();

                    waitForOrAssert(() => ipc != null, @"ipc could not be populated in a reasonable amount of time");

                    Assert.True(ipc.SetIPCLocation(testCeDir));
                    Assert.True(storage.AllTournaments.Exists("stable.json"));
                }
                finally
                {
                    host.Storage.DeleteDirectory(testCeDir);
                    host.Storage.DeleteDirectory("tournaments");
                    host.Exit();
                }
            }
        }

        private TournamentGameBase loadOsu(GameHost host)
        {
            var osu = new TournamentGameBase();
            Task.Run(() => host.Run(osu));
            waitForOrAssert(() => osu.IsLoaded, @"osu! failed to start in a reasonable amount of time");
            return osu;
        }

        private static void waitForOrAssert(Func<bool> result, string failureMessage, int timeout = 90000)
        {
            Task task = Task.Run(() =>
            {
                while (!result()) Thread.Sleep(200);
            });

            Assert.IsTrue(task.Wait(timeout), failureMessage);
        }
    }
}
