// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Components
{
    /// <summary>
    /// A <see cref="RoomPollingComponent"/> that polls for the currently-selected room.
    /// </summary>
    public class SelectionPollingComponent : RoomPollingComponent
    {
        private readonly Room room;

        public SelectionPollingComponent(Room room)
        {
            this.room = room;
        }

        private GetRoomRequest pollReq;

        protected override Task Poll()
        {
            if (!API.IsLoggedIn)
                return base.Poll();

            if (room.RoomID.Value == null)
                return base.Poll();

            var tcs = new TaskCompletionSource<bool>();

            pollReq?.Cancel();
            pollReq = new GetRoomRequest(room.RoomID.Value.Value);

            pollReq.Success += result =>
            {
                result.RemoveExpiredPlaylistItems();
                RoomManager.AddOrUpdateRoom(result);
                tcs.SetResult(true);
            };

            pollReq.Failure += _ => tcs.SetResult(false);

            API.Queue(pollReq);

            return tcs.Task;
        }
    }
}
