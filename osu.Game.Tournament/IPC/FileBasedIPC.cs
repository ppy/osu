// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Win32;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.IPC
{
    public class FileBasedIPC : MatchIPCInfo
    {
        public Storage IPCStorage { get; private set; }

        [Resolved]
        protected IAPIProvider API { get; private set; }

        [Resolved]
        protected IRulesetStore Rulesets { get; private set; }

        [Resolved]
        private GameHost host { get; set; }

        [Resolved]
        private LadderInfo ladder { get; set; }

        [Resolved]
        private StableInfo stableInfo { get; set; }

        private int lastBeatmapId;
        private ScheduledDelegate scheduled;
        private GetBeatmapRequest beatmapLookupRequest;

        [BackgroundDependencyLoader]
        private void load()
        {
            string stablePath = stableInfo.StablePath ?? findStablePath();
            initialiseIPCStorage(stablePath);
        }

        [CanBeNull]
        private Storage initialiseIPCStorage(string path)
        {
            scheduled?.Cancel();

            IPCStorage = null;

            try
            {
                if (string.IsNullOrEmpty(path))
                    return null;

                IPCStorage = new DesktopStorage(path, host as DesktopGameHost);

                const string file_ipc_filename = "ipc.txt";
                const string file_ipc_state_filename = "ipc-state.txt";
                const string file_ipc_scores_filename = "ipc-scores.txt";
                const string file_ipc_channel_filename = "ipc-channel.txt";

                if (IPCStorage.Exists(file_ipc_filename))
                {
                    scheduled = Scheduler.AddDelayed(delegate
                    {
                        try
                        {
                            using (var stream = IPCStorage.GetStream(file_ipc_filename))
                            using (var sr = new StreamReader(stream))
                            {
                                int beatmapId = int.Parse(sr.ReadLine().AsNonNull());
                                int mods = int.Parse(sr.ReadLine().AsNonNull());

                                if (lastBeatmapId != beatmapId)
                                {
                                    beatmapLookupRequest?.Cancel();

                                    lastBeatmapId = beatmapId;

                                    var existing = ladder.CurrentMatch.Value?.Round.Value?.Beatmaps.FirstOrDefault(b => b.ID == beatmapId && b.Beatmap != null);

                                    if (existing != null)
                                        Beatmap.Value = existing.Beatmap;
                                    else
                                    {
                                        beatmapLookupRequest = new GetBeatmapRequest(new APIBeatmap { OnlineID = beatmapId });
                                        beatmapLookupRequest.Success += b => Beatmap.Value = b;
                                        API.Queue(beatmapLookupRequest);
                                    }
                                }

                                Mods.Value = (LegacyMods)mods;
                            }
                        }
                        catch
                        {
                            // file might be in use.
                        }

                        try
                        {
                            using (var stream = IPCStorage.GetStream(file_ipc_channel_filename))
                            using (var sr = new StreamReader(stream))
                            {
                                ChatChannel.Value = sr.ReadLine();
                            }
                        }
                        catch (Exception)
                        {
                            // file might be in use.
                        }

                        try
                        {
                            using (var stream = IPCStorage.GetStream(file_ipc_state_filename))
                            using (var sr = new StreamReader(stream))
                            {
                                State.Value = (TourneyState)Enum.Parse(typeof(TourneyState), sr.ReadLine().AsNonNull());
                            }
                        }
                        catch (Exception)
                        {
                            // file might be in use.
                        }

                        try
                        {
                            using (var stream = IPCStorage.GetStream(file_ipc_scores_filename))
                            using (var sr = new StreamReader(stream))
                            {
                                Score1.Value = int.Parse(sr.ReadLine());
                                Score2.Value = int.Parse(sr.ReadLine());
                            }
                        }
                        catch (Exception)
                        {
                            // file might be in use.
                        }
                    }, 250, true);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Stable installation could not be found; disabling file based IPC");
            }

            return IPCStorage;
        }

        /// <summary>
        /// Manually sets the path to the directory used for inter-process communication with a cutting-edge install.
        /// </summary>
        /// <param name="path">Path to the IPC directory</param>
        /// <returns>Whether the supplied path was a valid IPC directory.</returns>
        public bool SetIPCLocation(string path)
        {
            if (path == null || !ipcFileExistsInDirectory(path))
                return false;

            var newStorage = initialiseIPCStorage(stableInfo.StablePath = path);
            if (newStorage == null)
                return false;

            stableInfo.SaveChanges();
            return true;
        }

        /// <summary>
        /// Tries to automatically detect the path to the directory used for inter-process communication
        /// with a cutting-edge install.
        /// </summary>
        /// <returns>Whether an IPC directory was successfully auto-detected.</returns>
        public bool AutoDetectIPCLocation() => SetIPCLocation(findStablePath());

        private static bool ipcFileExistsInDirectory(string p) => p != null && File.Exists(Path.Combine(p, "ipc.txt"));

        [CanBeNull]
        private string findStablePath()
        {
            string stableInstallPath = findFromEnvVar() ??
                                       findFromRegistry() ??
                                       findFromLocalAppData() ??
                                       findFromDotFolder();

            Logger.Log($"Stable path for tourney usage: {stableInstallPath}");
            return stableInstallPath;
        }

        private string findFromEnvVar()
        {
            try
            {
                Logger.Log("Trying to find stable with environment variables");
                string stableInstallPath = Environment.GetEnvironmentVariable("OSU_STABLE_PATH");

                if (ipcFileExistsInDirectory(stableInstallPath))
                    return stableInstallPath;
            }
            catch
            {
            }

            return null;
        }

        private string findFromLocalAppData()
        {
            Logger.Log("Trying to find stable in %LOCALAPPDATA%");
            string stableInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"osu!");

            if (ipcFileExistsInDirectory(stableInstallPath))
                return stableInstallPath;

            return null;
        }

        private string findFromDotFolder()
        {
            Logger.Log("Trying to find stable in dotfolders");
            string stableInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".osu");

            if (ipcFileExistsInDirectory(stableInstallPath))
                return stableInstallPath;

            return null;
        }

        private string findFromRegistry()
        {
            Logger.Log("Trying to find stable in registry");

            try
            {
                string stableInstallPath;

                using (RegistryKey key = Registry.ClassesRoot.OpenSubKey("osu"))
                    stableInstallPath = key?.OpenSubKey(@"shell\open\command")?.GetValue(string.Empty)?.ToString()?.Split('"')[1].Replace("osu!.exe", "");

                if (ipcFileExistsInDirectory(stableInstallPath))
                    return stableInstallPath;
            }
            catch
            {
            }

            return null;
        }
    }
}
