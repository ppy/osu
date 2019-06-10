// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using System;
using System.IO;
using System.Threading.Tasks;

namespace osu.Game.Screens.Play
{
    class ReplayPlayerLoader : PlayerLoader
    {
        private readonly ScoreInfo score;

        private Score replayScore = null;

        private Func<Score, Player> createPlayerWithReplay;

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        public ReplayPlayerLoader(ScoreInfo score, Func<Score, Player> createPlayerWithReplay) : base(null)
        {
            this.score = score;
            this.createPlayerWithReplay = createPlayerWithReplay;
        }

        protected virtual Score CreateReplayScore(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                return new Score
                {
                    ScoreInfo = score,
                    Replay = new DatabasedLegacyScoreParser(rulesets, beatmaps).Parse(stream).Replay,
                };
            }
        }

        protected void LoadReplay(string filename, Action<Player> onLoad)
        {
            replayScore = CreateReplayScore(filename);

            var player = createPlayerWithReplay(replayScore);

            LoadComponentAsync(player, onLoad);
        }

        protected override Task CreatePlayerLoadTask(Action<Player> onLoad)
        {
            return Task.Run(() =>
            {
                var request = new DownloadReplayRequest(score);

                request.Success += filename =>
                {
                    LoadReplay(filename, onLoad);
                };

                request.Failure += e =>
                {
                    Logger.Error(e, @"Replay download failed!");
                    Schedule(() => this.Exit());
                };

                // TODO: check whether framework is supposed to throw when file not found for download (as it is doing now) or trigger failure
                try
                {
                    request.Perform(api);
                }
                catch (Exception e)
                {
                    Logger.Error(e, @"Replay download failed!");
                    Schedule(() => this.Exit());
                }
            });
        }
    }
}
