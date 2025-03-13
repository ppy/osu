// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Tests.Beatmaps;
using osu.Game.Utils;

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
        /// Handles an API request, while also updating the local state to match how the server would eventually respond.
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
                    apiRoom.Password = createRoomRequest.Room.Password;

                    AddServerSideRoom(apiRoom, localUser);

                    var responseRoom = new APICreatedRoom();
                    responseRoom.CopyFrom(createResponseRoom(apiRoom, false));

                    createRoomRequest.TriggerSuccess(responseRoom);
                    return true;

                case JoinRoomRequest joinRoomRequest:
                {
                    var room = ServerSideRooms.Single(r => r.RoomID == joinRoomRequest.Room.RoomID);

                    if (joinRoomRequest.Password != room.Password)
                    {
                        joinRoomRequest.TriggerFailure(new InvalidOperationException("Invalid password."));
                        return true;
                    }

                    joinRoomRequest.TriggerSuccess(createResponseRoom(room, true));
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

                case IndexPlaylistScoresRequest roomLeaderboardRequest:
                    roomLeaderboardRequest.TriggerSuccess(new IndexedMultiplayerScores
                    {
                        Scores =
                        {
                            new MultiplayerScore
                            {
                                ID = currentScoreId++,
                                Accuracy = 1,
                                Position = 1,
                                EndedAt = DateTimeOffset.Now,
                                Passed = true,
                                Rank = ScoreRank.S,
                                MaxCombo = 1000,
                                TotalScore = 1000000,
                                User = new APIUser { Username = "best user" },
                                Mods = [new APIMod { Acronym = @"DT" }],
                                Statistics = new Dictionary<HitResult, int>()
                            },
                            new MultiplayerScore
                            {
                                ID = currentScoreId++,
                                Accuracy = 0.7,
                                Position = 2,
                                EndedAt = DateTimeOffset.Now,
                                Passed = true,
                                Rank = ScoreRank.B,
                                MaxCombo = 100,
                                TotalScore = 200000,
                                User = new APIUser { Username = "worst user" },
                                Mods = [new APIMod { Acronym = @"TD" }],
                                Statistics = new Dictionary<HitResult, int>()
                            },
                        },
                        UserScore = new MultiplayerScore
                        {
                            ID = currentScoreId++,
                            Accuracy = 0.91,
                            Position = 4,
                            EndedAt = DateTimeOffset.Now,
                            Passed = true,
                            Rank = ScoreRank.A,
                            MaxCombo = 100,
                            TotalScore = 800000,
                            User = localUser,
                            Statistics = new Dictionary<HitResult, int>()
                        },
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
                    getRoomRequest.TriggerSuccess(createResponseRoom(ServerSideRooms.Single(r => r.RoomID == getRoomRequest.RoomId), true));
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

                case GetBeatmapRequest getBeatmapRequest:
                {
                    getBeatmapRequest.TriggerSuccess(createResponseBeatmaps(getBeatmapRequest.OnlineID).Single());
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
                        : beatmapManager.QueryBeatmapSet(s => s.OnlineID == getBeatmapSetRequest.ID)?.PerformRead(s => s.Beatmaps.First().Detach());

                    if (baseBeatmap == null)
                    {
                        baseBeatmap = new TestBeatmap(new RulesetInfo { OnlineID = 0 }).BeatmapInfo;
                        baseBeatmap.OnlineID = getBeatmapSetRequest.ID;
                        baseBeatmap.BeatmapSet!.OnlineID = getBeatmapSetRequest.ID;
                    }

                    getBeatmapSetRequest.TriggerSuccess(OsuTestScene.CreateAPIBeatmapSet(baseBeatmap));
                    return true;
                }

                case GetUsersRequest getUsersRequest:
                {
                    getUsersRequest.TriggerSuccess(new GetUsersResponse
                    {
                        Users = getUsersRequest.UserIds.Select(id => id == TestUserLookupCache.UNRESOLVED_USER_ID
                                                   ? null
                                                   : new APIUser
                                                   {
                                                       Id = id,
                                                       Username = $"User {id}",
                                                       Team = RNG.NextBool()
                                                           ? new APITeam
                                                           {
                                                               Name = "Collective Wangs",
                                                               ShortName = "WANG",
                                                               FlagUrl = "https://assets.ppy.sh/teams/flag/1/wanglogo.jpg",
                                                           }
                                                           : null,
                                                   })
                                               .Where(u => u != null).ToList(),
                    });
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
            room.RoomID ??= currentRoomId++;
            room.Host = host;

            for (int i = 0; i < room.Playlist.Count; i++)
            {
                room.Playlist[i].ID = currentPlaylistItemId++;
                room.Playlist[i].OwnerID = room.Host.OnlineID;
            }

            serverSideRooms.Add(room);
        }

        private Room createResponseRoom(Room room, bool withParticipants)
        {
            var responseRoom = cloneRoom(room);

            // Password is hidden from the response, and is only propagated via HasPassword.
            responseRoom.Password = responseRoom.HasPassword ? Guid.NewGuid().ToString() : null;

            if (!withParticipants)
                responseRoom.RecentParticipants = [];

            return responseRoom;
        }

        private Room cloneRoom(Room source)
        {
            var result = JsonConvert.DeserializeObject<Room>(JsonConvert.SerializeObject(source));
            Debug.Assert(result != null);

            // When serialising, only beatmap IDs are sent to the server.
            // When deserialising, full beatmaps and IDs are expected to arrive.

            PlaylistItem? finalCurrentItem = result.CurrentPlaylistItem?.With(id: source.CurrentPlaylistItem!.ID, beatmap: new Optional<IBeatmapInfo>(source.CurrentPlaylistItem.Beatmap));
            PlaylistItem[] finalPlaylist = result.Playlist.Select((pi, i) => pi.With(id: source.Playlist[i].ID, beatmap: new Optional<IBeatmapInfo>(source.Playlist[i].Beatmap))).ToArray();

            // When setting the properties, we do a clear-then-add, otherwise equality comparers (that only compare by ID) pass early and members don't get replaced.
            result.CurrentPlaylistItem = null;
            result.CurrentPlaylistItem = finalCurrentItem;
            result.Playlist = [];
            result.Playlist = finalPlaylist;

            return result;
        }
    }
}
