// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Screens.Multi.Match.Components;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMatchLeaderboard : MultiplayerTestScene
    {
        protected override bool UseOnlineAPI => true;

        public TestSceneMatchLeaderboard()
        {
            Room.RoomID.Value = 3;

            Add(new MatchLeaderboard
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Size = new Vector2(550f, 450f),
                Scope = MatchLeaderboardScope.Overall,
            });
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            var req = new GetRoomScoresRequest();
            req.Success += v => { };
            req.Failure += _ => { };

            api.Queue(req);
        }

        private class GetRoomScoresRequest : APIRequest<List<RoomScore>>
        {
            protected override string Target => "rooms/3/leaderboard";
        }

        private class RoomScore
        {
            [JsonProperty("user")]
            public User User { get; set; }

            [JsonProperty("accuracy")]
            public double Accuracy { get; set; }

            [JsonProperty("total_score")]
            public int TotalScore { get; set; }

            [JsonProperty("pp")]
            public double PP { get; set; }

            [JsonProperty("attempts")]
            public int TotalAttempts { get; set; }

            [JsonProperty("completed")]
            public int CompletedAttempts { get; set; }
        }
    }
}
