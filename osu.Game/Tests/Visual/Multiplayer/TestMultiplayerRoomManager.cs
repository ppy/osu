// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Multiplayer;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestMultiplayerRoomManager : MultiplayerRoomManager
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private OsuGameBase game { get; set; }

        [Cached]
        public readonly Bindable<FilterCriteria> Filter = new Bindable<FilterCriteria>(new FilterCriteria());

        private readonly List<Room> rooms = new List<Room>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            int currentScoreId = 0;
            int currentRoomId = 0;

            ((DummyAPIAccess)api).HandleRequest = req =>
            {
                switch (req)
                {
                    case CreateRoomRequest createRoomRequest:
                        var createdRoom = new APICreatedRoom();

                        createdRoom.CopyFrom(createRoomRequest.Room);
                        createdRoom.RoomID.Value ??= currentRoomId++;

                        rooms.Add(createdRoom);
                        createRoomRequest.TriggerSuccess(createdRoom);
                        break;

                    case JoinRoomRequest joinRoomRequest:
                        joinRoomRequest.TriggerSuccess();
                        break;

                    case PartRoomRequest partRoomRequest:
                        partRoomRequest.TriggerSuccess();
                        break;

                    case GetRoomsRequest getRoomsRequest:
                        var roomsWithoutParticipants = new List<Room>();

                        foreach (var r in rooms)
                        {
                            var newRoom = new Room();

                            newRoom.CopyFrom(r);
                            newRoom.RecentParticipants.Clear();

                            roomsWithoutParticipants.Add(newRoom);
                        }

                        getRoomsRequest.TriggerSuccess(roomsWithoutParticipants);
                        break;

                    case GetRoomRequest getRoomRequest:
                        getRoomRequest.TriggerSuccess(rooms.Single(r => r.RoomID.Value == getRoomRequest.RoomId));
                        break;

                    case GetBeatmapSetRequest getBeatmapSetRequest:
                        var onlineReq = new GetBeatmapSetRequest(getBeatmapSetRequest.ID, getBeatmapSetRequest.Type);
                        onlineReq.Success += res => getBeatmapSetRequest.TriggerSuccess(res);
                        onlineReq.Failure += e => getBeatmapSetRequest.TriggerFailure(e);

                        // Get the online API from the game's dependencies.
                        game.Dependencies.Get<IAPIProvider>().Queue(onlineReq);
                        break;

                    case CreateRoomScoreRequest createRoomScoreRequest:
                        createRoomScoreRequest.TriggerSuccess(new APIScoreToken { ID = 1 });
                        break;

                    case SubmitRoomScoreRequest submitRoomScoreRequest:
                        submitRoomScoreRequest.TriggerSuccess(new MultiplayerScore
                        {
                            ID = currentScoreId++,
                            Accuracy = 1,
                            EndedAt = DateTimeOffset.Now,
                            Passed = true,
                            Rank = ScoreRank.S,
                            MaxCombo = 1000,
                            TotalScore = 1000000,
                            User = api.LocalUser.Value,
                            Statistics = new Dictionary<HitResult, int>()
                        });
                        break;
                }
            };
        }

        public new void ClearRooms() => base.ClearRooms();

        public new void Schedule(Action action) => base.Schedule(action);
    }
}
