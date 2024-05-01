// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Tests.Beatmaps;

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
        /// <param name="beatmapManager">The beatmap manager to attempt to retrieve beatmaps from, prior to returning dummy beatmaps.</param>
        /// <returns>Whether the request was successfully handled.</returns>
        public bool HandleRequest(APIRequest request, APIUser localUser, BeatmapManager beatmapManager)
        {
            switch (request)
            {
                case CreateRoomRequest createRoomRequest:
                    var apiRoom = cloneRoom(createRoomRequest.Room);

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

                case GetRoomLeaderboardRequest roomLeaderboardRequest:
                    roomLeaderboardRequest.TriggerSuccess(new APILeaderboard
                    {
                        Leaderboard = new List<APIUserScoreAggregate>
                        {
                            new APIUserScoreAggregate
                            {
                                TotalScore = 1000000,
                                TotalAttempts = 5,
                                CompletedBeatmaps = 2,
                                User = new APIUser { Username = "best user" }
                            },
                            new APIUserScoreAggregate
                            {
                                TotalScore = 50,
                                TotalAttempts = 1,
                                CompletedBeatmaps = 1,
                                User = new APIUser { Username = "worst user" }
                            }
                        }
                    });
                    return true;

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
                        Rank = ScoreRank.S,
                        MaxCombo = 1000,
                        TotalScore = 1000000,
                        User = localUser,
                        Statistics = new Dictionary<HitResult, int>()
                    });
                    return true;

                case GetBeatmapRequest getBeatmapRequest:
                {
                    getBeatmapRequest.TriggerSuccess(createResponseBeatmaps(getBeatmapRequest.BeatmapInfo.OnlineID).Single());
                    return true;
                }

                case GetBeatmapsRequest getBeatmapsRequest:
                {
                    getBeatmapsRequest.TriggerSuccess(new GetBeatmapsResponse { Beatmaps = createResponseBeatmaps(getBeatmapsRequest.BeatmapIds.ToArray()) });
                    return true;
                }

                case GetBeatmapSetRequest getBeatmapSetRequest:
                {
                    var baseBeatmap = getBeatmapSetRequest.Type == BeatmapSetLookupType.BeatmapId
                        ? beatmapManager.QueryBeatmap(b => b.OnlineID == getBeatmapSetRequest.ID)
                        : beatmapManager.QueryBeatmap(b => b.BeatmapSet.OnlineID == getBeatmapSetRequest.ID);

                    if (baseBeatmap == null)
                    {
                        baseBeatmap = new TestBeatmap(new RulesetInfo { OnlineID = 0 }).BeatmapInfo;
                        baseBeatmap.OnlineID = getBeatmapSetRequest.ID;
                        baseBeatmap.BeatmapSet!.OnlineID = getBeatmapSetRequest.ID;
                    }

                    getBeatmapSetRequest.TriggerSuccess(OsuTestScene.CreateAPIBeatmapSet(baseBeatmap));
                    return true;
                }
            }

            List<APIBeatmap> createResponseBeatmaps(params int[] beatmapIds)
            {
                var result = new List<APIBeatmap>();

                foreach (int id in beatmapIds)
                {
                    var baseBeatmap = beatmapManager.QueryBeatmap(b => b.OnlineID == id);

                    if (baseBeatmap == null)
                    {
                        baseBeatmap = new TestBeatmap(new RulesetInfo { OnlineID = 0 }).BeatmapInfo;
                        baseBeatmap.OnlineID = id;
                        baseBeatmap.BeatmapSet!.OnlineID = id;
                    }

                    result.Add(OsuTestScene.CreateAPIBeatmap(baseBeatmap));
                }

                return result;
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
            var responseRoom = cloneRoom(room);

            // Password is hidden from the response, and is only propagated via HasPassword.
            bool hadPassword = responseRoom.HasPassword.Value;
            responseRoom.Password.Value = null;
            responseRoom.HasPassword.Value = hadPassword;

            if (!withParticipants)
                responseRoom.RecentParticipants.Clear();

            return responseRoom;
        }

        private Room cloneRoom(Room source)
        {
            var result = JsonConvert.DeserializeObject<Room>(JsonConvert.SerializeObject(source));
            Debug.Assert(result != null);

            // Playlist item IDs aren't serialised.
            if (source.CurrentPlaylistItem.Value != null)
                result.CurrentPlaylistItem.Value.ID = source.CurrentPlaylistItem.Value.ID;
            for (int i = 0; i < source.Playlist.Count; i++)
                result.Playlist[i].ID = source.Playlist[i].ID;

            return result;
        }
    }
}
