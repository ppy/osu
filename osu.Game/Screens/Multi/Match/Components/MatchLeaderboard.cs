// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Online.Multiplayer;
using osu.Game.Scoring;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class MatchLeaderboard : Leaderboard<MatchLeaderboardScope, APIRoomScoreInfo>
    {
        public Action<IEnumerable<APIRoomScoreInfo>> ScoresLoaded;

        private readonly Room room;

        public MatchLeaderboard(Room room)
        {
            this.room = room;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            room.RoomID.BindValueChanged(id =>
            {
                if (id == null)
                    return;

                Scores = null;
                UpdateScores();
            }, true);
        }

        protected override APIRequest FetchScores(Action<IEnumerable<APIRoomScoreInfo>> scoresCallback)
        {
            if (room.RoomID == null)
                return null;

            var req = new GetRoomScoresRequest(room.RoomID.Value ?? 0);

            req.Success += r =>
            {
                scoresCallback?.Invoke(r);
                ScoresLoaded?.Invoke(r);
            };

            return req;
        }

        protected override LeaderboardScore CreateDrawableScore(APIRoomScoreInfo model, int index) => new MatchLeaderboardScore(model, index);
    }

    public class MatchLeaderboardScore : LeaderboardScore
    {
        public MatchLeaderboardScore(APIRoomScoreInfo score, int rank)
            : base(score, rank)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RankContainer.Alpha = 0;
        }

        protected override IEnumerable<LeaderboardScoreStatistic> GetStatistics(ScoreInfo model) => new[]
        {
            new LeaderboardScoreStatistic(FontAwesome.fa_crosshairs, "Accuracy", string.Format(model.Accuracy % 1 == 0 ? @"{0:P0}" : @"{0:P2}", model.Accuracy)),
            new LeaderboardScoreStatistic(FontAwesome.fa_refresh, "Total Attempts", ((APIRoomScoreInfo)model).TotalAttempts.ToString()),
            new LeaderboardScoreStatistic(FontAwesome.fa_check, "Completed Beatmaps", ((APIRoomScoreInfo)model).CompletedBeatmaps.ToString()),
        };
    }

    public enum MatchLeaderboardScope
    {
        Overall
    }
}
