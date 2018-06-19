// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
