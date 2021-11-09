// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public class RoomManager : Component, IRoomManager
    {
        public event Action RoomsUpdated;

        private readonly BindableList<Room> rooms = new BindableList<Room>();

        public IBindableList<Room> Rooms => rooms;

        protected IBindable<Room> JoinedRoom => joinedRoom;
        private readonly Bindable<Room> joinedRoom = new Bindable<Room>();

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        public RoomManager()
        {
            RelativeSizeAxes = Axes.Both;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            PartRoom();
        }

        public virtual void CreateRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null)
        {
            room.Host.Value = api.LocalUser.Value;

            var req = new CreateRoomRequest(room);

            req.Success += result =>
            {
                joinedRoom.Value = room;

                AddOrUpdateRoom(result);
                room.CopyFrom(result); // Also copy back to the source model, since this is likely to have been stored elsewhere.

                // The server may not contain all properties (such as password), so invoke success with the given room.
                onSuccess?.Invoke(room);
            };

            req.Failure += exception =>
            {
                onError?.Invoke(req.Response?.Error ?? exception.Message);
            };

            api.Queue(req);
        }

        private JoinRoomRequest currentJoinRoomRequest;

        public virtual void JoinRoom(Room room, string password = null, Action<Room> onSuccess = null, Action<string> onError = null)
        {
            currentJoinRoomRequest?.Cancel();
            currentJoinRoomRequest = new JoinRoomRequest(room, password);

            currentJoinRoomRequest.Success += () =>
            {
                joinedRoom.Value = room;
                onSuccess?.Invoke(room);
            };

            currentJoinRoomRequest.Failure += exception =>
            {
                if (exception is OperationCanceledException)
                    return;

                onError?.Invoke(exception.Message);
            };

            api.Queue(currentJoinRoomRequest);
        }

        public virtual void PartRoom()
        {
            currentJoinRoomRequest?.Cancel();

            if (JoinedRoom.Value == null)
                return;

            api.Queue(new PartRoomRequest(joinedRoom.Value));
            joinedRoom.Value = null;
        }

        private readonly HashSet<long> ignoredRooms = new HashSet<long>();

        public void AddOrUpdateRoom(Room room)
        {
            Debug.Assert(room.RoomID.Value != null);

            if (ignoredRooms.Contains(room.RoomID.Value.Value))
                return;

            try
            {
                foreach (var pi in room.Playlist)
                    pi.MapObjects(rulesets);

                var existing = rooms.FirstOrDefault(e => e.RoomID.Value == room.RoomID.Value);
                if (existing == null)
                    rooms.Add(room);
                else
                    existing.CopyFrom(room);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to update room: {room.Name.Value}.");

                ignoredRooms.Add(room.RoomID.Value.Value);
                rooms.Remove(room);
            }

            notifyRoomsUpdated();
        }

        public void RemoveRoom(Room room)
        {
            rooms.Remove(room);
            notifyRoomsUpdated();
        }

        public void ClearRooms()
        {
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
