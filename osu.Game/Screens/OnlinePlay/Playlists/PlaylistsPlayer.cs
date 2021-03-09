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
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public class PlaylistsPlayer : Player
    {
        public Action Exited;

        [Resolved(typeof(Room), nameof(Room.RoomID))]
        protected Bindable<long?> RoomId { get; private set; }

        protected readonly PlaylistItem PlaylistItem;

        protected int? Token { get; private set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        public PlaylistsPlayer(PlaylistItem playlistItem, PlayerConfiguration configuration = null)
            : base(configuration)
        {
            PlaylistItem = playlistItem;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Token = null;

            bool failed = false;

            // Sanity checks to ensure that PlaylistsPlayer matches the settings for the current PlaylistItem
            if (Beatmap.Value.BeatmapInfo.OnlineBeatmapID != PlaylistItem.Beatmap.Value.OnlineBeatmapID)
                throw new InvalidOperationException("当前谱面与游玩列表不匹配"); //Current Beatmap does not match PlaylistItem's Beatmap

            if (ruleset.Value.ID != PlaylistItem.Ruleset.Value.ID)
                throw new InvalidOperationException("当前游戏模式与游玩列表不匹配"); //Current Ruleset does not match PlaylistItem's Ruleset

            if (!PlaylistItem.RequiredMods.All(m => Mods.Value.Any(m.Equals)))
                throw new InvalidOperationException("当前Mods与游玩列表所需要的不匹配"); //Current Mods do not match PlaylistItem's RequiredMods

            var req = new CreateRoomScoreRequest(RoomId.Value ?? 0, PlaylistItem.ID, Game.VersionHash);
            req.Success += r => Token = r.ID;
            req.Failure += e =>
            {
                failed = true;

                if (string.IsNullOrEmpty(e.Message))
                    Logger.Error(e, "无法获取用于提交分数的token。");
                else
                    Logger.Log($"你无法提交分数： {e.Message}", level: LogLevel.Important);

                Schedule(() =>
                {
                    ValidForResume = false;
                    this.Exit();
                });
            };

            api.Queue(req);

            while (!failed && !Token.HasValue)
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
            Debug.Assert(RoomId.Value != null);
            return new PlaylistsResultsScreen(score, RoomId.Value.Value, PlaylistItem, true);
        }

        protected override Score CreateScore()
        {
            var score = base.CreateScore();
            score.ScoreInfo.TotalScore = (int)Math.Round(ScoreProcessor.GetStandardisedScore());
            return score;
        }

        protected override async Task SubmitScore(Score score)
        {
            await base.SubmitScore(score).ConfigureAwait(false);

            Debug.Assert(Token != null);

            var tcs = new TaskCompletionSource<bool>();
            var request = new SubmitRoomScoreRequest(Token.Value, RoomId.Value ?? 0, PlaylistItem.ID, score.ScoreInfo);

            request.Success += s =>
            {
                score.ScoreInfo.OnlineScoreID = s.ID;
                tcs.SetResult(true);
            };

            request.Failure += e =>
            {
                Logger.Error(e, "无法提交分数");
                tcs.SetResult(false);
            };

            api.Queue(request);
            await tcs.Task.ConfigureAwait(false);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Exited = null;
        }
    }
}
