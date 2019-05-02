// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetUserMostPlayedBeatmapsRequest : APIRequest<List<APIUserMostPlayedBeatmap>>
    {
        private readonly long userId;
        private readonly int offset;

        public GetUserMostPlayedBeatmapsRequest(long userId, int offset = 0)
        {
            this.userId = userId;
            this.offset = offset;
        }

        protected override string Target => $@"users/{userId}/beatmapsets/most_played?offset={offset}";
    }
}
