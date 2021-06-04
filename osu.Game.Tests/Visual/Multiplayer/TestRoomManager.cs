// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestRoomManager : IRoomManager
    {
        public event Action RoomsUpdated
        {
            add { }
            remove { }
        }

        public readonly BindableList<Room> Rooms = new BindableList<Room>();

        public IBindable<bool> InitialRoomsReceived { get; } = new Bindable<bool>(true);

        IBindableList<Room> IRoomManager.Rooms => Rooms;

        public void CreateRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null) => Rooms.Add(room);

        public void JoinRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null)
        {
        }

        public void PartRoom()
        {
        }
    }
}
