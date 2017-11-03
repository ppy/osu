// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;

namespace osu.Game.Online.API.Requests
{
    public class GetUserBeatmapsRequest : APIRequest<List<GetBeatmapSetsResponse>>
    {
        private readonly long userId;
        private readonly BeatmapSetType type;
        private readonly int offset;

        public GetUserBeatmapsRequest(long userId, BeatmapSetType type, int offset = 0)
        {
            this.userId = userId;
            this.type = type;
            this.offset = offset;
        }

        protected override string Target => $@"users/{userId}/beatmapsets/{type.ToString().ToLower()}?offset={offset}";
    }

    public enum BeatmapSetType
    {
        Most_Played,
        Favourite,
        Ranked_And_Approved
    }
}
