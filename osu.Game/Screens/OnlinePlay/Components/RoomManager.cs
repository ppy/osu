// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public partial class RoomManager : Component, IRoomManager
    {
        public event Action? RoomsUpdated;

        private readonly BindableList<Room> rooms = new BindableList<Room>();

        public IBindableList<Room> Rooms => rooms;

        public RoomManager()
        {
            RelativeSizeAxes = Axes.Both;
        }

        private readonly HashSet<long> ignoredRooms = new HashSet<long>();

        public void AddOrUpdateRoom(Room room)
        {
            Debug.Assert(ThreadSafety.IsUpdateThread);
            Debug.Assert(room.RoomID != null);

            if (ignoredRooms.Contains(room.RoomID.Value))
                return;

            try
            {
                var existing = rooms.FirstOrDefault(e => e.RoomID == room.RoomID);
                if (existing == null)
                    rooms.Add(room);
                else
                    existing.CopyFrom(room);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to update room: {room.Name}.");

                ignoredRooms.Add(room.RoomID.Value);
                rooms.Remove(room);
            }

            notifyRoomsUpdated();
        }

        public void RemoveRoom(Room room)
        {
            Debug.Assert(ThreadSafety.IsUpdateThread);

            rooms.Remove(room);
            notifyRoomsUpdated();
        }

        public void ClearRooms()
        {
            Debug.Assert(ThreadSafety.IsUpdateThread);

            rooms.Clear();
            notifyRoomsUpdated();
        }

        private void notifyRoomsUpdated()
        {
            Scheduler.AddOnce(invokeRoomsUpdated);

            void invokeRoomsUpdated() => RoomsUpdated?.Invoke();
        }
    }
}
