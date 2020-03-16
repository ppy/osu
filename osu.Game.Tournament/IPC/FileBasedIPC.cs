// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Platform.Windows;
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

        public Storage Storage { get; private set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            LocateStableStorage();
        }

        public Storage LocateStableStorage()
        {
            scheduled?.Cancel();

            Storage = null;

            try
            {
                Storage = new StableStorage(host as DesktopGameHost);

                const string file_ipc_filename = "ipc.txt";
                const string file_ipc_state_filename = "ipc-state.txt";
                const string file_ipc_scores_filename = "ipc-scores.txt";
                const string file_ipc_channel_filename = "ipc-channel.txt";

                if (Storage.Exists(file_ipc_filename))
                {
                    scheduled = Scheduler.AddDelayed(delegate
                    {
                        try
                        {
                            using (var stream = Storage.GetStream(file_ipc_filename))
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
                            using (var stream = Storage.GetStream(file_ipc_channel_filename))
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
                            using (var stream = Storage.GetStream(file_ipc_state_filename))
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
                            using (var stream = Storage.GetStream(file_ipc_scores_filename))
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

            return Storage;
        }

        /// <summary>
        /// A method of accessing an osu-stable install in a controlled fashion.
        /// </summary>
        private class StableStorage : WindowsStorage
        {
            protected override string LocateBasePath()
            {
                static bool checkExists(string p)
                {
                    return File.Exists(Path.Combine(p, "ipc.txt"));
                }

                string stableInstallPath = string.Empty;

                try
                {
                    try
                    {
                        stableInstallPath = Environment.GetEnvironmentVariable("OSU_STABLE_PATH");

                        if (checkExists(stableInstallPath))
                            return stableInstallPath;
                    }
                    catch
                    {
                    }

                    try
                    {
                        using (RegistryKey key = Registry.ClassesRoot.OpenSubKey("osu"))
                            stableInstallPath = key?.OpenSubKey(@"shell\open\command")?.GetValue(string.Empty).ToString().Split('"')[1].Replace("osu!.exe", "");

                        if (checkExists(stableInstallPath))
                            return stableInstallPath;
                    }
                    catch
                    {
                    }

                    stableInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"osu!");
                    if (checkExists(stableInstallPath))
                        return stableInstallPath;

                    stableInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".osu");
                    if (checkExists(stableInstallPath))
                        return stableInstallPath;

                    return null;
                }
                finally
                {
                    Logger.Log($"Stable path for tourney usage: {stableInstallPath}");
                }
            }

            public StableStorage(DesktopGameHost host)
                : base(string.Empty, host)
            {
            }
        }
    }
}
