// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Humanizer;
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

            if (statusFilter != RoomStatusFilter.Open)
                req.AddParameter("mode", statusFilter.ToString().Underscore().ToLowerInvariant());

            if (categoryFilter != RoomCategoryFilter.Any)
                req.AddParameter("category", categoryFilter.ToString().Underscore().ToLowerInvariant());

            return req;
        }

        protected override string Target => "rooms";
    }
}
