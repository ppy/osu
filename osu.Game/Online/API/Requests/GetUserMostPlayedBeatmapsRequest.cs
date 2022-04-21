// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetUserMostPlayedBeatmapsRequest : PaginatedAPIRequest<List<APIUserMostPlayedBeatmap>>
    {
        private readonly long userId;

        public GetUserMostPlayedBeatmapsRequest(long userId, PaginationParameters pagination)
            : base(pagination)
        {
            this.userId = userId;
        }

        protected override string Target => $@"users/{userId}/beatmapsets/most_played";
    }
}
