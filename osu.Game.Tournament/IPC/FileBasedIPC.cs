// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Framework.Platform.Windows;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;

namespace osu.Game.Tournament.IPC
{
    public class FileBasedIPC : MatchIPCInfo
    {
        [Resolved]
        protected APIAccess API { get; private set; }

        [Resolved]
        protected RulesetStore Rulesets { get; private set; }

        private int lastBeatmapId;

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            StableStorage stable;

            try
            {
                stable = new StableStorage();
            }
            catch
            {
                Logger.Log("Stable installation could not be found; disabling file based IPC");
                return;
            }

            const string file_ipc_filename = "ipc.txt";
            const string file_ipc_state_filename = "ipc-state.txt";
            const string file_ipc_scores_filename = "ipc-scores.txt";
            const string file_ipc_channel_filename = "ipc-channel.txt";

            if (stable.Exists(file_ipc_filename))
                Scheduler.AddDelayed(delegate
                {
                    try
                    {
                        using (var stream = stable.GetStream(file_ipc_filename))
                        using (var sr = new StreamReader(stream))
                        {
                            var beatmapId = int.Parse(sr.ReadLine());
                            var mods = int.Parse(sr.ReadLine());

                            if (lastBeatmapId != beatmapId)
                            {
                                lastBeatmapId = beatmapId;

                                var existing = ladder.CurrentMatch.Value?.Grouping.Value?.Beatmaps.FirstOrDefault(b => b.ID == beatmapId && b.BeatmapInfo != null);

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
                        using (var stream = stable.GetStream(file_ipc_channel_filename))
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
                        using (var stream = stable.GetStream(file_ipc_state_filename))
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
                        using (var stream = stable.GetStream(file_ipc_scores_filename))
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

        /// <summary>
        /// A method of accessing an osu-stable install in a controlled fashion.
        /// </summary>
        private class StableStorage : WindowsStorage
        {
            protected override string LocateBasePath()
            {

                bool checkExists(string p)
                {
                    return File.Exists(Path.Combine(p, "ipc.txt"));
                }

                string stableInstallPath = string.Empty;

                try
                {
                    try
                    {
                        stableInstallPath = "E:\\osu!tourney";

                        if (checkExists(stableInstallPath))
                            return stableInstallPath;

                        stableInstallPath = "E:\\osu!mappool";

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

            public StableStorage()
                : base(string.Empty, null)
            {
            }
        }
    }
}
