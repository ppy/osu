// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens.Multi.Ranking;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Multi.Play
{
    public class TimeshiftPlayer : Player
    {
        public Action Exited;

        [Resolved(typeof(Room), nameof(Room.RoomID))]
        private Bindable<int?> roomId { get; set; }

        private readonly PlaylistItem playlistItem;

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        public TimeshiftPlayer(PlaylistItem playlistItem)
        {
            this.playlistItem = playlistItem;
        }

        private int? token;

        [BackgroundDependencyLoader]
        private void load()
        {
            token = null;

            bool failed = false;

            // Sanity checks to ensure that TimeshiftPlayer matches the settings for the current PlaylistItem
            if (Beatmap.Value.BeatmapInfo.OnlineBeatmapID != playlistItem.Beatmap.Value.OnlineBeatmapID)
                throw new InvalidOperationException("Current Beatmap does not match PlaylistItem's Beatmap");

            if (ruleset.Value.ID != playlistItem.Ruleset.Value.ID)
                throw new InvalidOperationException("Current Ruleset does not match PlaylistItem's Ruleset");

            if (!playlistItem.RequiredMods.All(m => Mods.Value.Any(m.Equals)))
                throw new InvalidOperationException("Current Mods do not match PlaylistItem's RequiredMods");

            var req = new CreateRoomScoreRequest(roomId.Value ?? 0, playlistItem.ID, Game.VersionHash);
            req.Success += r => token = r.ID;
            req.Failure += e =>
            {
                failed = true;

                Logger.Error(e, "Failed to retrieve a score submission token.\n\nThis may happen if you are running an old or non-official release of osu! (ie. you are self-compiling).");

                Schedule(() =>
                {
                    ValidForResume = false;
                    this.Exit();
                });
            };

            api.Queue(req);

            while (!failed && !token.HasValue)
                Thread.Sleep(1000);
        }

        public override bool OnExiting(IScreen next)
        {
            if (base.OnExiting(next))
                return true;

            Exited?.Invoke();

            return false;
        }

        protected override ResultsScreen CreateResults(ScoreInfo score)
        {
            Debug.Assert(roomId.Value != null);
            return new TimeshiftResultsScreen(score, roomId.Value.Value, playlistItem, true);
        }

        protected override Score CreateScore()
        {
            var score = base.CreateScore();
            score.ScoreInfo.TotalScore = (int)Math.Round(ScoreProcessor.GetStandardisedScore());
            return score;
        }

        protected override async Task SubmitScore(Score score)
        {
            await base.SubmitScore(score);

            Debug.Assert(token != null);

            var tcs = new TaskCompletionSource<bool>();
            var request = new SubmitRoomScoreRequest(token.Value, roomId.Value ?? 0, playlistItem.ID, score.ScoreInfo);

            request.Success += s =>
            {
                score.ScoreInfo.OnlineScoreID = s.ID;
                tcs.SetResult(true);
            };

            request.Failure += e =>
            {
                Logger.Error(e, "Failed to submit score");
                tcs.SetResult(false);
            };

            api.Queue(request);
            await tcs.Task;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Exited = null;
        }
    }
}
