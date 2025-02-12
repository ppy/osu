// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Lounge.Components;

namespace osu.Game.Screens.OnlinePlay.Components
{
    /// <summary>
    /// A <see cref="RoomPollingComponent"/> that polls for the lounge listing.
    /// </summary>
    public partial class ListingPollingComponent : RoomPollingComponent
    {
        public required Action<Room[]> RoomsReceived { get; init; }
        public readonly IBindable<FilterCriteria?> Filter = new Bindable<FilterCriteria?>();

        private GetRoomsRequest? lastPollRequest;

        protected override Task Poll()
        {
            if (!API.IsLoggedIn)
                return base.Poll();

            if (Filter.Value == null)
                return base.Poll();

            lastPollRequest?.Cancel();

            var tcs = new TaskCompletionSource<bool>();
            var req = new GetRoomsRequest(Filter.Value);

            req.Success += result =>
            {
                RoomsReceived(result.Where(r => r.Category != RoomCategory.DailyChallenge).ToArray());
                tcs.SetResult(true);
            };

            req.Failure += _ => tcs.SetResult(false);

            API.Queue(req);

            lastPollRequest = req;

            return tcs.Task;
        }
    }
}
