// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.Leaderboards;
using osu.Game.Online.Multiplayer;
using osu.Game.Scoring;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class MatchLeaderboard : Leaderboard<MatchLeaderboardScope, RoomScore>
    {
        public Action<IEnumerable<RoomScore>> ScoresLoaded;

        private readonly Room room;

        public MatchLeaderboard(Room room)
        {
            this.room = room;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            room.RoomID.BindValueChanged(_ =>
            {
                Scores = null;
                UpdateScores();
            }, true);
        }

        protected override APIRequest FetchScores(Action<IEnumerable<RoomScore>> scoresCallback)
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

        protected override LeaderboardScore CreateDrawableScore(RoomScore model, int index) => new MatchLeaderboardScore(model, index);

        private class GetRoomScoresRequest : APIRequest<List<RoomScore>>
        {
            private readonly int roomId;

            public GetRoomScoresRequest(int roomId)
            {
                this.roomId = roomId;
            }

            protected override string Target => $@"rooms/{roomId}/leaderboard";
        }
    }

    public class MatchLeaderboardScore : LeaderboardScore
    {
        public MatchLeaderboardScore(RoomScore score, int rank)
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
            new LeaderboardScoreStatistic(FontAwesome.fa_refresh, "Total Attempts", ((RoomScore)model).TotalAttempts.ToString()),
            new LeaderboardScoreStatistic(FontAwesome.fa_check, "Completed Beatmaps", ((RoomScore)model).CompletedAttempts.ToString()),
        };
    }

    public enum MatchLeaderboardScope
    {
        Overall
    }

    public class RoomScore : ScoreInfo
    {
        [JsonProperty("attempts")]
        public int TotalAttempts { get; set; }

        [JsonProperty("completed")]
        public int CompletedAttempts { get; set; }
    }
}
