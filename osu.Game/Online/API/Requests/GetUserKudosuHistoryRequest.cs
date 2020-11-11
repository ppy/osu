// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetUserKudosuHistoryRequest : PaginatedAPIRequest<List<APIKudosuHistory>>
    {
        private readonly long userId;

        public GetUserKudosuHistoryRequest(long userId, int page = 0, int itemsPerPage = 5)
            : base(page, itemsPerPage)
        {
            this.userId = userId;
        }

        protected override string Target => $"users/{userId}/kudosu";
    }
}
