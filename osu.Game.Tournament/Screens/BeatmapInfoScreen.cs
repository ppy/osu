// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform.Windows;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Screens;
using osu.Game.Tournament.Components;

namespace osu.Game.Tournament.Screens
{
    public abstract class BeatmapInfoScreen : OsuScreen
    {
        [Resolved]
        protected APIAccess API { get; private set; }

        [Resolved]
        protected RulesetStore Rulesets { get; private set; }

        private int lastBeatmapId;
        private int lastMods;

        protected readonly SongBar SongBar;

        protected BeatmapInfoScreen()
        {
            Add(SongBar = new SongBar
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre
            });
        }

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

                            if (lastBeatmapId == beatmapId)
                                return;

                            lastMods = mods;
                            lastBeatmapId = beatmapId;

                            var req = new GetBeatmapRequest(new BeatmapInfo { OnlineBeatmapID = beatmapId });
                            req.Success += success;
                            API.Queue(req);
                        }
                    }
                    catch
                    {
                        // file might be in use.
                    }
                }, 250, true);
        }

        private void success(APIBeatmap apiBeatmap)
        {
            SongBar.FadeInFromZero(300, Easing.OutQuint);
            SongBar.Mods = (LegacyMods)lastMods;
            SongBar.Beatmap = apiBeatmap.ToBeatmap(Rulesets);
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
