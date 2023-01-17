// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Threading.Tasks;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Components
{
    /// <summary>
    /// A <see cref="RoomPollingComponent"/> that polls for the currently-selected room.
    /// </summary>
    public partial class SelectionPollingComponent : RoomPollingComponent
    {
        private readonly Room room;

        public SelectionPollingComponent(Room room)
        {
            this.room = room;
        }

        private GetRoomRequest lastPollRequest;

        protected override Task Poll()
        {
            if (!API.IsLoggedIn)
                return base.Poll();

            if (room.RoomID.Value == null)
                return base.Poll();

            var tcs = new TaskCompletionSource<bool>();

            lastPollRequest?.Cancel();

            var req = new GetRoomRequest(room.RoomID.Value.Value);

            req.Success += result =>
            {
                result.RemoveExpiredPlaylistItems();
                RoomManager.AddOrUpdateRoom(result);
                tcs.SetResult(true);
            };

            req.Failure += _ => tcs.SetResult(false);

            API.Queue(req);

            lastPollRequest = req;

            return tcs.Task;
        }
    }
}
