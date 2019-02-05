// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;

namespace osu.Game.Screens.Multi
{
    public class RoomManager : Container, IRoomManager
    {
        public event Action RoomsUpdated;

        private readonly BindableList<Room> rooms = new BindableList<Room>();
        public IBindableList<Room> Rooms => rooms;

        private Room currentRoom;

        [Resolved]
        private APIAccess api { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            PartRoom();
        }

        public void CreateRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null)
        {
            room.Host.Value = api.LocalUser;

            var req = new CreateRoomRequest(room);

            req.Success += result =>
            {
                update(room, result);
                addRoom(room);

                RoomsUpdated?.Invoke();

                onSuccess?.Invoke(room);
            };

            req.Failure += exception =>
            {
                if (req.Result != null)
                    onError?.Invoke(req.Result.Error);
                else
                    Logger.Log($"Failed to create the room: {exception}", level: LogLevel.Important);
            };

            api.Queue(req);
        }

        private JoinRoomRequest currentJoinRoomRequest;

        public void JoinRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null)
        {
            currentJoinRoomRequest?.Cancel();
            currentJoinRoomRequest = null;

            currentJoinRoomRequest = new JoinRoomRequest(room, api.LocalUser.Value);
            currentJoinRoomRequest.Success += () =>
            {
                currentRoom = room;
                onSuccess?.Invoke(room);
            };

            currentJoinRoomRequest.Failure += exception =>
            {
                Logger.Log($"Failed to join room: {exception}", level: LogLevel.Important);
                onError?.Invoke(exception.ToString());
            };

            api.Queue(currentJoinRoomRequest);
        }

        public void PartRoom()
        {
            if (currentRoom == null)
                return;

            api.Queue(new PartRoomRequest(currentRoom, api.LocalUser.Value));
            currentRoom = null;
        }

        public void UpdateRooms(List<Room> newRooms)
        {
            // Remove past matches
            foreach (var r in rooms.ToList())
            {
                if (newRooms.All(e => e.RoomID.Value != r.RoomID.Value))
                    rooms.Remove(r);
            }

            for (int i = 0; i < newRooms.Count; i++)
            {
                var r = newRooms[i];
                r.Position = i;

                update(r, r);
                addRoom(r);
            }

            RoomsUpdated?.Invoke();
        }

        /// <summary>
        /// Updates a local <see cref="Room"/> with a remote copy.
        /// </summary>
        /// <param name="local">The local <see cref="Room"/> to update.</param>
        /// <param name="remote">The remote <see cref="Room"/> to update with.</param>
        private void update(Room local, Room remote)
        {
            foreach (var pi in remote.Playlist)
                pi.MapObjects(beatmaps, rulesets);

            local.CopyFrom(remote);
        }

        /// <summary>
        /// Adds a <see cref="Room"/> to the list of available rooms.
        /// </summary>
        /// <param name="room">The <see cref="Room"/> to add.<</param>
        private void addRoom(Room room)
        {
            var existing = rooms.FirstOrDefault(e => e.RoomID.Value == room.RoomID.Value);
            if (existing == null)
                rooms.Add(room);
            else
                existing.CopyFrom(room);
        }
    }
}
