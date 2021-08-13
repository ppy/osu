// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Lounge.Components;

namespace osu.Game.Screens.OnlinePlay.Components
{
    /// <summary>
    /// A <see cref="RoomPollingComponent"/> that polls for the lounge listing.
    /// </summary>
    public class ListingPollingComponent : RoomPollingComponent
    {
        [Resolved]
        private Bindable<FilterCriteria> currentFilter { get; set; }

        [Resolved]
        private Bindable<Room> selectedRoom { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            currentFilter.BindValueChanged(_ =>
            {
                NotifyRoomsReceived(null);
                if (IsLoaded)
                    PollImmediately();
            });
        }

        private GetRoomsRequest pollReq;

        protected override Task Poll()
        {
            if (!API.IsLoggedIn)
                return base.Poll();

            var tcs = new TaskCompletionSource<bool>();

            pollReq?.Cancel();
            pollReq = new GetRoomsRequest(currentFilter.Value.Status, currentFilter.Value.Category);

            pollReq.Success += result =>
            {
                for (int i = 0; i < result.Count; i++)
                {
                    if (result[i].RoomID.Value == selectedRoom.Value?.RoomID.Value)
                    {
                        // The listing request always has less information than the opened room, so don't include it.
                        result[i] = selectedRoom.Value;
                        break;
                    }
                }

                NotifyRoomsReceived(result);
                tcs.SetResult(true);
            };

            pollReq.Failure += _ => tcs.SetResult(false);

            API.Queue(pollReq);

            return tcs.Task;
        }
    }
}
