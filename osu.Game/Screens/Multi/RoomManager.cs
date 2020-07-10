// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
    public class RoomManager : CompositeDrawable, IRoomManager
    {
        public event Action RoomsUpdated;

        private readonly BindableList<Room> rooms = new BindableList<Room>();

        public Bindable<bool> InitialRoomsReceived { get; } = new Bindable<bool>();

        public IBindableList<Room> Rooms => rooms;

        public double TimeBetweenListingPolls
        {
            get => listingPollingComponent.TimeBetweenPolls;
            set => listingPollingComponent.TimeBetweenPolls = value;
        }

        public double TimeBetweenSelectionPolls
        {
            get => selectionPollingComponent.TimeBetweenPolls;
            set => selectionPollingComponent.TimeBetweenPolls = value;
        }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private Bindable<Room> selectedRoom { get; set; }

        private readonly ListingPollingComponent listingPollingComponent;
        private readonly SelectionPollingComponent selectionPollingComponent;

        private Room joinedRoom;

        public RoomManager()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                listingPollingComponent = new ListingPollingComponent
                {
                    InitialRoomsReceived = { BindTarget = InitialRoomsReceived },
                    RoomsReceived = onListingReceived
                },
                selectionPollingComponent = new SelectionPollingComponent { RoomReceived = onSelectedRoomReceived }
            };
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            PartRoom();
        }

        public void CreateRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null)
        {
            room.Host.Value = api.LocalUser.Value;

            var req = new CreateRoomRequest(room);

            req.Success += result =>
            {
                joinedRoom = room;

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
            currentJoinRoomRequest = new JoinRoomRequest(room, api.LocalUser.Value);

            currentJoinRoomRequest.Success += () =>
            {
                joinedRoom = room;
                onSuccess?.Invoke(room);
            };

            currentJoinRoomRequest.Failure += exception =>
            {
                if (!(exception is OperationCanceledException))
                    Logger.Log($"Failed to join room: {exception}", level: LogLevel.Important);
                onError?.Invoke(exception.ToString());
            };

            api.Queue(currentJoinRoomRequest);
        }

        public void PartRoom()
        {
            currentJoinRoomRequest?.Cancel();

            if (joinedRoom == null)
                return;

            api.Queue(new PartRoomRequest(joinedRoom, api.LocalUser.Value));
            joinedRoom = null;
        }

        private readonly HashSet<int> ignoredRooms = new HashSet<int>();

        /// <summary>
        /// Invoked when the listing of all <see cref="Room"/>s is received from the server.
        /// </summary>
        /// <param name="listing">The listing.</param>
        private void onListingReceived(List<Room> listing)
        {
            // Remove past matches
            foreach (var r in rooms.ToList())
            {
                if (listing.All(e => e.RoomID.Value != r.RoomID.Value))
                    rooms.Remove(r);
            }

            for (int i = 0; i < listing.Count; i++)
            {
                if (selectedRoom.Value?.RoomID?.Value == listing[i].RoomID.Value)
                {
                    // The listing request contains less data than the selection request, so data from the selection request is always preferred while the room is selected.
                    continue;
                }

                var room = listing[i];

                Debug.Assert(room.RoomID.Value != null);

                if (ignoredRooms.Contains(room.RoomID.Value.Value))
                    continue;

                room.Position.Value = i;

                try
                {
                    update(room, room);
                    addRoom(room);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Failed to update room: {room.Name.Value}.");

                    ignoredRooms.Add(room.RoomID.Value.Value);
                    rooms.Remove(room);
                }
            }

            RoomsUpdated?.Invoke();
        }

        /// <summary>
        /// Invoked when a <see cref="Room"/> is received from the server.
        /// </summary>
        /// <param name="toUpdate">The received <see cref="Room"/>.</param>
        private void onSelectedRoomReceived(Room toUpdate)
        {
            foreach (var room in rooms)
            {
                if (room.RoomID.Value == toUpdate.RoomID.Value)
                {
                    toUpdate.Position.Value = room.Position.Value;
                    update(room, toUpdate);
                    break;
                }
            }
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
        /// <param name="room">The <see cref="Room"/> to add.</param>
        private void addRoom(Room room)
        {
            var existing = rooms.FirstOrDefault(e => e.RoomID.Value == room.RoomID.Value);
            if (existing == null)
                rooms.Add(room);
            else
                existing.CopyFrom(room);
        }

        private class SelectionPollingComponent : PollingComponent
        {
            public Action<Room> RoomReceived;

            [Resolved]
            private IAPIProvider api { get; set; }

            [Resolved]
            private Bindable<Room> selectedRoom { get; set; }

            [BackgroundDependencyLoader]
            private void load()
            {
                selectedRoom.BindValueChanged(_ =>
                {
                    if (IsLoaded)
                        PollImmediately();
                });
            }

            private GetRoomRequest pollReq;

            protected override Task Poll()
            {
                if (!api.IsLoggedIn)
                    return base.Poll();

                if (selectedRoom.Value?.RoomID.Value == null)
                    return base.Poll();

                var tcs = new TaskCompletionSource<bool>();

                pollReq?.Cancel();
                pollReq = new GetRoomRequest(selectedRoom.Value.RoomID.Value.Value);

                pollReq.Success += result =>
                {
                    RoomReceived?.Invoke(result);
                    tcs.SetResult(true);
                };

                pollReq.Failure += _ => tcs.SetResult(false);

                api.Queue(pollReq);

                return tcs.Task;
            }
        }

        private class ListingPollingComponent : PollingComponent
        {
            public Action<List<Room>> RoomsReceived;

            public readonly Bindable<bool> InitialRoomsReceived = new Bindable<bool>();

            [Resolved]
            private IAPIProvider api { get; set; }

            [Resolved]
            private Bindable<FilterCriteria> currentFilter { get; set; }

            [BackgroundDependencyLoader]
            private void load()
            {
                currentFilter.BindValueChanged(_ =>
                {
                    InitialRoomsReceived.Value = false;

                    if (IsLoaded)
                        PollImmediately();
                });
            }

            private GetRoomsRequest pollReq;

            protected override Task Poll()
            {
                if (!api.IsLoggedIn)
                    return base.Poll();

                var tcs = new TaskCompletionSource<bool>();

                pollReq?.Cancel();
                pollReq = new GetRoomsRequest(currentFilter.Value.StatusFilter, currentFilter.Value.RoomCategoryFilter);

                pollReq.Success += result =>
                {
                    InitialRoomsReceived.Value = true;
                    RoomsReceived?.Invoke(result);
                    tcs.SetResult(true);
                };

                pollReq.Failure += _ => tcs.SetResult(false);

                api.Queue(pollReq);

                return tcs.Task;
            }
        }
    }
}
