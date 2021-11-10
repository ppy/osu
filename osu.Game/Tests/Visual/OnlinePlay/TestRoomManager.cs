// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay.Components;

namespace osu.Game.Tests.Visual.OnlinePlay
{
    /// <summary>
    /// A very simple <see cref="RoomManager"/> for use in online play test scenes.
    /// </summary>
    public class TestRoomManager : RoomManager
    {
        public Action<Room, string> JoinRoomRequested;

        private int currentRoomId;

        public override void JoinRoom(Room room, string password = null, Action<Room> onSuccess = null, Action<string> onError = null)
        {
            JoinRoomRequested?.Invoke(room, password);
            base.JoinRoom(room, password, onSuccess, onError);
        }

        public void AddRooms(int count, RulesetInfo ruleset = null, bool withPassword = false)
        {
            for (int i = 0; i < count; i++)
            {
                var room = new Room
                {
                    RoomID = { Value = -currentRoomId },
                    Name = { Value = $@"Room {currentRoomId}" },
                    Host = { Value = new APIUser { Username = @"Host" } },
                    EndDate = { Value = DateTimeOffset.Now + TimeSpan.FromSeconds(10) },
                    Category = { Value = i % 2 == 0 ? RoomCategory.Spotlight : RoomCategory.Normal },
                };

                if (withPassword)
                    room.Password.Value = @"password";

                if (ruleset != null)
                {
                    room.Playlist.Add(new PlaylistItem
                    {
                        Ruleset = { Value = ruleset },
                        Beatmap =
                        {
                            Value = new BeatmapInfo
                            {
                                Metadata = new BeatmapMetadata()
                            }
                        }
                    });
                }

                CreateRoom(room);

                currentRoomId++;
            }
        }
    }
}
