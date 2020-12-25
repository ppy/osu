// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Components
{
    /// <summary>
    /// A <see cref="RoomPollingComponent"/> that polls for the currently-selected room.
    /// </summary>
    public class SelectionPollingComponent : RoomPollingComponent
    {
        [Resolved]
        private Bindable<Room> selectedRoom { get; set; }

        [Resolved]
        private IRoomManager roomManager { get; set; }

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
            if (!API.IsLoggedIn)
                return base.Poll();

            if (selectedRoom.Value?.RoomID.Value == null)
                return base.Poll();

            var tcs = new TaskCompletionSource<bool>();

            pollReq?.Cancel();
            pollReq = new GetRoomRequest(selectedRoom.Value.RoomID.Value.Value);

            pollReq.Success += result =>
            {
                var rooms = new List<Room>(roomManager.Rooms);

                int index = rooms.FindIndex(r => r.RoomID.Value == result.RoomID.Value);
                if (index < 0)
                    return;

                rooms[index] = result;

                NotifyRoomsReceived(rooms);
                tcs.SetResult(true);
            };

            pollReq.Failure += _ => tcs.SetResult(false);

            API.Queue(pollReq);

            return tcs.Task;
        }
    }
}
