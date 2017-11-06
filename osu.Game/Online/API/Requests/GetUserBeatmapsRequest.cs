// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;

namespace osu.Game.Online.API.Requests
{
    public class GetUserBeatmapsRequest : APIRequest<List<GetBeatmapSetsResponse>>
    {
        private readonly long userId;
        private readonly int offset;
        private readonly string type;

        public GetUserBeatmapsRequest(long userId, BeatmapSetType type, int offset = 0)
        {
            this.userId = userId;
            this.offset = offset;

            switch (type)
            {
                case BeatmapSetType.Favourite:
                    this.type = type.ToString().ToLower();
                    break;
                case BeatmapSetType.MostPlayed:
                    this.type = "most_played";
                    break;
                case BeatmapSetType.RankedAndApproved:
                    this.type = "ranked_and_approved";
                    break;
            }
        }

        protected override string Target => $@"users/{userId}/beatmapsets/{type}?offset={offset}";
    }

    public enum BeatmapSetType
    {
        MostPlayed,
        Favourite,
        RankedAndApproved
    }
}
