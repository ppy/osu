// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Tests.Visual.OnlinePlay;
using osuTK;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneMatchLeaderboard : OnlinePlayTestScene
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("setup API", () =>
            {
                ((DummyAPIAccess)API).HandleRequest = r =>
                {
                    switch (r)
                    {
                        case GetRoomLeaderboardRequest leaderboardRequest:
                            leaderboardRequest.TriggerSuccess(new APILeaderboard
                            {
                                Leaderboard = new List<APIUserScoreAggregate>
                                {
                                    new APIUserScoreAggregate
                                    {
                                        UserID = 2,
                                        User = new APIUser { Id = 2, Username = "peppy" },
                                        TotalScore = 995533,
                                        RoomID = 3,
                                        CompletedBeatmaps = 1,
                                        TotalAttempts = 6,
                                        Accuracy = 0.9851
                                    },
                                    new APIUserScoreAggregate
                                    {
                                        UserID = 1040328,
                                        User = new APIUser { Id = 1040328, Username = "smoogipoo" },
                                        TotalScore = 981100,
                                        RoomID = 3,
                                        CompletedBeatmaps = 1,
                                        TotalAttempts = 9,
                                        Accuracy = 0.937
                                    }
                                }
                            });
                            return true;
                    }

                    return false;
                };
            });

            AddStep("create leaderboard", () =>
            {
                SelectedRoom.Value = new Room { RoomID = { Value = 3 } };

                Child = new MatchLeaderboard
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Size = new Vector2(550f, 450f),
                    Scope = MatchLeaderboardScope.Overall,
                };
            });
        }
    }
}
