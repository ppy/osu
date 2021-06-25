// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.OnlinePlay
{
    public class BasicTestRoomManager : IRoomManager
    {
        public event Action RoomsUpdated
        {
            add { }
            remove { }
        }

        public readonly BindableList<Room> Rooms = new BindableList<Room>();

        public IBindable<bool> InitialRoomsReceived { get; } = new Bindable<bool>(true);

        IBindableList<Room> IRoomManager.Rooms => Rooms;

        public void CreateRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null)
        {
            room.RoomID.Value ??= Rooms.Select(r => r.RoomID.Value).Where(id => id != null).Select(id => id.Value).DefaultIfEmpty().Max() + 1;
            Rooms.Add(room);
            onSuccess?.Invoke(room);
        }

        public void JoinRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null) => onSuccess?.Invoke(room);

        public void PartRoom()
        {
        }

        public void AddRooms(int count, RulesetInfo ruleset = null)
        {
            for (int i = 0; i < count; i++)
            {
                var room = new Room
                {
                    RoomID = { Value = i },
                    Name = { Value = $"Room {i}" },
                    Host = { Value = new User { Username = "Host" } },
                    EndDate = { Value = DateTimeOffset.Now + TimeSpan.FromSeconds(10) },
                    Category = { Value = i % 2 == 0 ? RoomCategory.Spotlight : RoomCategory.Normal }
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
