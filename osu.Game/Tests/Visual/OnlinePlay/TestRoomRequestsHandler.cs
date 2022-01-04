// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Components;

namespace osu.Game.Tests.Visual.OnlinePlay
{
    /// <summary>
    /// Represents a handler which pretends to be a server, handling room retrieval and manipulation requests
    /// and returning a roughly expected state, without the need for a server to be running.
    /// </summary>
    public class TestRoomRequestsHandler
    {
        public IReadOnlyList<Room> ServerSideRooms => serverSideRooms;

        private readonly List<Room> serverSideRooms = new List<Room>();

        private int currentRoomId = 1;
        private int currentPlaylistItemId = 1;
        private int currentScoreId = 1;

        /// <summary>
        /// Handles an API request, while also updating the local state to match
        /// how the server would eventually respond and update an <see cref="RoomManager"/>.
        /// </summary>
        /// <param name="request">The API request to handle.</param>
        /// <param name="localUser">The local user to store in responses where required.</param>
        /// <param name="game">The game base for cases where actual online requests need to be sent.</param>
        /// <returns>Whether the request was successfully handled.</returns>
        public bool HandleRequest(APIRequest request, APIUser localUser, OsuGameBase game)
        {
            switch (request)
            {
                case CreateRoomRequest createRoomRequest:
                    var apiRoom = new Room();

                    apiRoom.CopyFrom(createRoomRequest.Room);

                    // Passwords are explicitly not copied between rooms.
                    apiRoom.HasPassword.Value = !string.IsNullOrEmpty(createRoomRequest.Room.Password.Value);
                    apiRoom.Password.Value = createRoomRequest.Room.Password.Value;

                    AddServerSideRoom(apiRoom, localUser);

                    var responseRoom = new APICreatedRoom();
                    responseRoom.CopyFrom(createResponseRoom(apiRoom, false));

                    createRoomRequest.TriggerSuccess(responseRoom);
                    return true;

                case JoinRoomRequest joinRoomRequest:
                {
                    var room = ServerSideRooms.Single(r => r.RoomID.Value == joinRoomRequest.Room.RoomID.Value);

                    if (joinRoomRequest.Password != room.Password.Value)
                    {
                        joinRoomRequest.TriggerFailure(new InvalidOperationException("Invalid password."));
                        return true;
                    }

                    joinRoomRequest.TriggerSuccess();
                    return true;
                }

                case PartRoomRequest partRoomRequest:
                    partRoomRequest.TriggerSuccess();
                    return true;

                case GetRoomsRequest getRoomsRequest:
                    var roomsWithoutParticipants = new List<Room>();

                    foreach (var r in ServerSideRooms)
                        roomsWithoutParticipants.Add(createResponseRoom(r, false));

                    getRoomsRequest.TriggerSuccess(roomsWithoutParticipants);
                    return true;

                case GetRoomRequest getRoomRequest:
                    getRoomRequest.TriggerSuccess(createResponseRoom(ServerSideRooms.Single(r => r.RoomID.Value == getRoomRequest.RoomId), true));
                    return true;

                case CreateRoomScoreRequest createRoomScoreRequest:
                    createRoomScoreRequest.TriggerSuccess(new APIScoreToken { ID = 1 });
                    return true;

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
                        User = localUser,
                        Statistics = new Dictionary<HitResult, int>()
                    });
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Adds a room to a local "server-side" list that's returned when a <see cref="GetRoomsRequest"/> is fired.
        /// </summary>
        /// <param name="room">The room.</param>
        /// <param name="host">The room host.</param>
        public void AddServerSideRoom(Room room, APIUser host)
        {
            room.RoomID.Value ??= currentRoomId++;
            room.Host.Value = host;

            for (int i = 0; i < room.Playlist.Count; i++)
            {
                room.Playlist[i].ID = currentPlaylistItemId++;
                room.Playlist[i].OwnerID = room.Host.Value.OnlineID;
            }

            serverSideRooms.Add(room);
        }

        private Room createResponseRoom(Room room, bool withParticipants)
        {
            var responseRoom = new Room();
            responseRoom.CopyFrom(room);
            responseRoom.Password.Value = null;
            if (!withParticipants)
                responseRoom.RecentParticipants.Clear();
            return responseRoom;
        }
    }
}
