// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Screens.Multi.Lounge.Components;

namespace osu.Game.Screens.Multi
{
    public class RoomManager : PollingComponent, IRoomManager
    {
        public event Action<Room> RoomJoined;

        private readonly BindableCollection<Room> rooms = new BindableCollection<Room>();
        public IBindableCollection<Room> Rooms => rooms;

        private Room currentRoom;

        private FilterCriteria currentFilter = new FilterCriteria();

        [Resolved]
        private APIAccess api { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        public RoomManager()
        {
            TimeBetweenPolls = 5000;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            PartRoom();
        }

        public void CreateRoom(Room room)
        {
            room.Host.Value = api.LocalUser;

            var req = new CreateRoomRequest(room);
            req.Success += result => addRoom(room, result);
            req.Failure += exception => Logger.Log($"Failed to create room: {exception}");

            api.Queue(req);
        }

        private JoinRoomRequest currentJoinRoomRequest;

        public void JoinRoom(Room room)
        {
            currentJoinRoomRequest?.Cancel();
            currentJoinRoomRequest = null;

            currentJoinRoomRequest = new JoinRoomRequest(room, api.LocalUser.Value);
            currentJoinRoomRequest.Success += () =>
            {
                currentRoom = room;
                RoomJoined?.Invoke(room);
            };

            currentJoinRoomRequest.Failure += exception => Logger.Log($"Failed to join room: {exception}");

            api.Queue(currentJoinRoomRequest);
        }

        public void PartRoom()
        {
            if (currentRoom == null)
                return;

            api.Queue(new PartRoomRequest(currentRoom, api.LocalUser.Value));
            currentRoom = null;
        }

        public void Filter(FilterCriteria criteria)
        {
            currentFilter = criteria;
            PollImmediately();
        }

        protected override Task Poll()
        {
            if (!api.IsLoggedIn)
                return base.Poll();

            var tcs = new TaskCompletionSource<bool>();

            var pollReq = new GetRoomsRequest(currentFilter.PrimaryFilter);

            pollReq.Success += result =>
            {
                // Remove past matches
                foreach (var r in rooms.ToList())
                {
                    if (result.All(e => e.RoomID.Value != r.RoomID.Value))
                        rooms.Remove(r);
                }

                // Add new matches, or update existing
                foreach (var r in result)
                {
                    processPlaylist(r);

                    var existing = rooms.FirstOrDefault(e => e.RoomID.Value == r.RoomID.Value);
                    if (existing == null)
                        rooms.Add(r);
                    else
                        existing.CopyFrom(r);
                }

                tcs.SetResult(true);
            };

            pollReq.Failure += _ => tcs.SetResult(false);

            api.Queue(pollReq);

            return tcs.Task;
        }

        private void addRoom(Room local, Room remote)
        {
            processPlaylist(remote);

            local.CopyFrom(remote);

            var existing = rooms.FirstOrDefault(e => e.RoomID.Value == local.RoomID.Value);
            if (existing != null)
                rooms.Remove(existing);
            rooms.Add(local);
        }

        private void processPlaylist(Room room)
        {
            foreach (var pi in room.Playlist)
                pi.MapObjects(beatmaps, rulesets);
        }
    }
}
