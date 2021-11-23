// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Game.Tournament.IO;
using osu.Game.Tournament.IPC;

namespace osu.Game.Tournament.Tests.NonVisual
{
    [TestFixture]
    public class IPCLocationTest : TournamentHostTest
    {
        [Test]
        public void CheckIPCLocation()
        {
            // don't use clean run because files are being written before osu! launches.
            using (HeadlessGameHost host = new HeadlessGameHost(nameof(CheckIPCLocation)))
            {
                string basePath = CustomTourneyDirectoryTest.PrepareBasePath(nameof(CheckIPCLocation));

                // Set up a fake IPC client for the IPC Storage to switch to.
                string testStableInstallDirectory = Path.Combine(basePath, "stable-ce");
                Directory.CreateDirectory(testStableInstallDirectory);

                string ipcFile = Path.Combine(testStableInstallDirectory, "ipc.txt");
                File.WriteAllText(ipcFile, string.Empty);

                try
                {
                    var osu = LoadTournament(host);
                    TournamentStorage storage = (TournamentStorage)osu.Dependencies.Get<Storage>();
                    FileBasedIPC ipc = null;

                    WaitForOrAssert(() => (ipc = osu.Dependencies.Get<MatchIPCInfo>() as FileBasedIPC) != null, @"ipc could not be populated in a reasonable amount of time");

                    Assert.True(ipc.SetIPCLocation(testStableInstallDirectory));
                    Assert.True(storage.AllTournaments.Exists("stable.json"));
                }
                finally
                {
                    host.Exit();

                    try
                    {
                        if (Directory.Exists(basePath))
                            Directory.Delete(basePath, true);
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
