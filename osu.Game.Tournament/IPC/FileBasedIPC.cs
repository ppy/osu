// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Win32;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.IPC
{
    public class FileBasedIPC : MatchIPCInfo
    {
        [Resolved]
        protected IAPIProvider API { get; private set; }

        [Resolved]
        protected RulesetStore Rulesets { get; private set; }

        [Resolved]
        private GameHost host { get; set; }

        [Resolved]
        private LadderInfo ladder { get; set; }

        private int lastBeatmapId;
        private ScheduledDelegate scheduled;

        [Resolved]
        private StableInfo stableInfo { get; set; }

        public Storage IPCStorage { get; private set; }

        [Resolved]
        private Storage tournamentStorage { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            LocateStableStorage();
        }

        public Storage LocateStableStorage()
        {
            scheduled?.Cancel();

            IPCStorage = null;

            try
            {
                var path = findStablePath();

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
                                var beatmapId = int.Parse(sr.ReadLine());
                                var mods = int.Parse(sr.ReadLine());

                                if (lastBeatmapId != beatmapId)
                                {
                                    lastBeatmapId = beatmapId;

                                    var existing = ladder.CurrentMatch.Value?.Round.Value?.Beatmaps.FirstOrDefault(b => b.ID == beatmapId && b.BeatmapInfo != null);

                                    if (existing != null)
                                        Beatmap.Value = existing.BeatmapInfo;
                                    else
                                    {
                                        var req = new GetBeatmapRequest(new BeatmapInfo { OnlineBeatmapID = beatmapId });
                                        req.Success += b => Beatmap.Value = b.ToBeatmap(Rulesets);
                                        API.Queue(req);
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
                                State.Value = (TourneyState)Enum.Parse(typeof(TourneyState), sr.ReadLine());
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

        public bool checkExists(string p) => File.Exists(Path.Combine(p, "ipc.txt"));

        private string findStablePath()
        {
            string stableInstallPath = string.Empty;

            try
            {
                List<Func<string>> stableFindMethods = new List<Func<string>>
                {
                    readFromStableInfo,
                    findFromEnvVar,
                    findFromRegistry,
                    findFromLocalAppData,
                    findFromDotFolder
                };

                foreach (var r in stableFindMethods)
                {
                    stableInstallPath = r.Invoke();

                    if (stableInstallPath != null)
                    {
                        saveStablePath(stableInstallPath);
                        return stableInstallPath;
                    }
                }

                return null;
            }
            finally
            {
                Logger.Log($"Stable path for tourney usage: {stableInstallPath}");
            }
        }

        private void saveStablePath(string path)
        {
            stableInfo.StablePath.Value = path;

            using (var stream = tournamentStorage.GetStream(StableInfo.STABLE_CONFIG, FileAccess.Write, FileMode.Create))
            using (var sw = new StreamWriter(stream))
            {
                sw.Write(JsonConvert.SerializeObject(stableInfo,
                    new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore,
                    }));
            }
        }

        private string findFromEnvVar()
        {
            try
            {
                Logger.Log("Trying to find stable with environment variables");
                string stableInstallPath = Environment.GetEnvironmentVariable("OSU_STABLE_PATH");

                if (checkExists(stableInstallPath))
                    return stableInstallPath;
            }
            catch
            {
            }

            return null;
        }

        private string readFromStableInfo()
        {
            try
            {
                Logger.Log("Trying to find stable through the json config");

                if (!string.IsNullOrEmpty(stableInfo.StablePath.Value))
                    return stableInfo.StablePath.Value;
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

            if (checkExists(stableInstallPath))
                return stableInstallPath;

            return null;
        }

        private string findFromDotFolder()
        {
            Logger.Log("Trying to find stable in dotfolders");
            string stableInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".osu");

            if (checkExists(stableInstallPath))
                return stableInstallPath;

            return null;
        }

        private string findFromRegistry()
        {
            Logger.Log("Trying to find stable in registry");

            string stableInstallPath;

            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey("osu"))
                stableInstallPath = key?.OpenSubKey(@"shell\open\command")?.GetValue(string.Empty).ToString().Split('"')[1].Replace("osu!.exe", "");

            if (checkExists(stableInstallPath))
                return stableInstallPath;

            return null;
        }
    }
}
