// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Platform.Windows;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;

namespace osu.Game.Tournament.IPC
{
    public class FileBasedIPC : Component
    {
        [Resolved]
        protected APIAccess API { get; private set; }

        [Resolved]
        protected RulesetStore Rulesets { get; private set; }

        public readonly Bindable<BeatmapInfo> Beatmap = new Bindable<BeatmapInfo>();

        public readonly Bindable<LegacyMods> Mods = new Bindable<LegacyMods>();

        [BackgroundDependencyLoader]
        private void load()
        {
            var stable = new StableStorage();

            const string file_ipc_filename = "ipc.txt";

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

                            if (Beatmap.Value?.OnlineBeatmapID != beatmapId)
                            {
                                var req = new GetBeatmapRequest(new BeatmapInfo { OnlineBeatmapID = beatmapId });
                                req.Success += b => Beatmap.Value = b.ToBeatmap(Rulesets);
                                API.Queue(req);
                            }

                            Mods.Value = (LegacyMods)mods;
                        }
                    }
                    catch
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
                    return Directory.Exists(Path.Combine(p, "Songs"));
                }

                string stableInstallPath;

                try
                {
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

            public StableStorage()
                : base(string.Empty, null)
            {
            }
        }
    }
}
