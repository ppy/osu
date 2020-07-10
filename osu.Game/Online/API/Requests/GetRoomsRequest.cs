// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.IO.Network;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Lounge.Components;

namespace osu.Game.Online.API.Requests
{
    public class GetRoomsRequest : APIRequest<List<Room>>
    {
        private readonly RoomStatusFilter statusFilter;
        private readonly RoomCategoryFilter categoryFilter;

        public GetRoomsRequest(RoomStatusFilter statusFilter, RoomCategoryFilter categoryFilter)
        {
            this.statusFilter = statusFilter;
            this.categoryFilter = categoryFilter;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            switch (statusFilter)
            {
                case RoomStatusFilter.Owned:
                    req.AddParameter("mode", "owned");
                    break;

                case RoomStatusFilter.Participated:
                    req.AddParameter("mode", "participated");
                    break;

                case RoomStatusFilter.RecentlyEnded:
                    req.AddParameter("mode", "ended");
                    break;
            }

            switch (categoryFilter)
            {
                case RoomCategoryFilter.Spotlight:
                    req.AddParameter("category", "spotlight");
                    break;
            }

            return req;
        }

        protected override string Target => "rooms";
    }
}
