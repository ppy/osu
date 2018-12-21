// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.Leaderboards;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Users;

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

        protected override LeaderboardScore<RoomScore> CreateScoreVisualiser(RoomScore model, int index) => new MatchLeaderboardScore(model, index);

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

    public class MatchLeaderboardScore : LeaderboardScore<RoomScore>
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

        protected override User GetUser(RoomScore model) => model.User;

        protected override IEnumerable<Mod> GetMods(RoomScore model) => Enumerable.Empty<Mod>(); // Not implemented yet

        protected override IEnumerable<(FontAwesome icon, string value, string name)> GetStatistics(RoomScore model) => new[]
        {
            (FontAwesome.fa_crosshairs, string.Format(model.Accuracy % 1 == 0 ? @"{0:P0}" : @"{0:P2}", model.Accuracy), "Accuracy"),
            (FontAwesome.fa_refresh, model.TotalAttempts.ToString(), "Total Attempts"),
            (FontAwesome.fa_check, model.CompletedAttempts.ToString(), "Completed Beatmaps"),
        };

        protected override int GetTotalScore(RoomScore model) => model.TotalScore;

        protected override ScoreRank GetRank(RoomScore model) => ScoreRank.S;
    }

    public enum MatchLeaderboardScope
    {
        Overall
    }

    public class RoomScore
    {
        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("accuracy")]
        public double Accuracy { get; set; }

        [JsonProperty("total_score")]
        public int TotalScore { get; set; }

        [JsonProperty("pp")]
        public double? PP { get; set; }

        [JsonProperty("attempts")]
        public int TotalAttempts { get; set; }

        [JsonProperty("completed")]
        public int CompletedAttempts { get; set; }
    }
}
