// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Lounge.Components;

namespace osu.Game.Screens.Multi
{
    public class RoomPollingComponent : PollingComponent
    {
        /// <summary>
        /// Invoked when <see cref="Room"/>s have been retrieved from the API.
        /// </summary>
        public Action<List<Room>> RoomsRetrieved;

        /// <summary>
        /// The <see cref="FilterCriteria"/> to use when polling for <see cref="Room"/>s.
        /// </summary>
        public readonly Bindable<FilterCriteria> Filter = new Bindable<FilterCriteria>();

        [Resolved]
        private APIAccess api { get; set; }

        public RoomPollingComponent()
        {
            Filter.BindValueChanged(_ =>
            {
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
            pollReq = new GetRoomsRequest(Filter.Value.PrimaryFilter);

            pollReq.Success += result =>
            {
                RoomsRetrieved?.Invoke(result);
                tcs.SetResult(true);
            };

            pollReq.Failure += _ => tcs.SetResult(false);

            api.Queue(pollReq);

            return tcs.Task;
        }
    }
}
