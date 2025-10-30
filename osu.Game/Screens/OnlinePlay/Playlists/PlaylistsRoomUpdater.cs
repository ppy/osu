// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    /// <summary>
    /// A <see cref="PollingComponent"/> that polls for and updates a room.
    /// </summary>
    public partial class PlaylistsRoomUpdater : PollingComponent
    {
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private readonly Room room;

        public PlaylistsRoomUpdater(Room room)
        {
            this.room = room;
        }

        private GetRoomRequest? lastPollRequest;

        protected override Task Poll()
        {
            if (!api.IsLoggedIn)
                return base.Poll();

            if (room.RoomID == null)
                return base.Poll();

            lastPollRequest?.Cancel();

            var tcs = new TaskCompletionSource<bool>();
            var req = new GetRoomRequest(room.RoomID.Value);

            req.Success += result =>
            {
                room.CopyFrom(result);
                tcs.SetResult(true);
            };

            req.Failure += _ => tcs.SetResult(false);

            api.Queue(req);

            lastPollRequest = req;

            return tcs.Task;
        }
    }
}
