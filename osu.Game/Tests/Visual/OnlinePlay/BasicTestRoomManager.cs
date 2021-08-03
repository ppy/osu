// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.OnlinePlay
{
    /// <summary>
    /// A very simple <see cref="RoomManager"/> for use in online play test scenes.
    /// </summary>
    public class BasicTestRoomManager : IRoomManager
    {
        public event Action RoomsUpdated;

        public readonly BindableList<Room> Rooms = new BindableList<Room>();

        public Action<Room, string> JoinRoomRequested;

        public IBindable<bool> InitialRoomsReceived { get; } = new Bindable<bool>(true);

        IBindableList<Room> IRoomManager.Rooms => Rooms;

        public void CreateRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null)
        {
            room.RoomID.Value ??= Rooms.Select(r => r.RoomID.Value).Where(id => id != null).Select(id => id.Value).DefaultIfEmpty().Max() + 1;
            onSuccess?.Invoke(room);

            AddRoom(room);
        }

        public void AddRoom(Room room)
        {
            Rooms.Add(room);
            RoomsUpdated?.Invoke();
        }

        public void RemoveRoom(Room room)
        {
            Rooms.Remove(room);
            RoomsUpdated?.Invoke();
        }

        public void JoinRoom(Room room, string password, Action<Room> onSuccess = null, Action<string> onError = null)
        {
            JoinRoomRequested?.Invoke(room, password);
            onSuccess?.Invoke(room);
        }

        public void PartRoom()
        {
        }

        public void AddRooms(int count, RulesetInfo ruleset = null, bool withPassword = false)
        {
            for (int i = 0; i < count; i++)
            {
                var room = new Room
                {
                    RoomID = { Value = i },
                    Position = { Value = i },
                    Name = { Value = $"Room {i}" },
                    Host = { Value = new User { Username = "Host" } },
                    EndDate = { Value = DateTimeOffset.Now + TimeSpan.FromSeconds(10) },
                    Category = { Value = i % 2 == 0 ? RoomCategory.Spotlight : RoomCategory.Normal },
                    Password = { Value = withPassword ? "password" : string.Empty }
                };

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
            }
        }
    }
}
