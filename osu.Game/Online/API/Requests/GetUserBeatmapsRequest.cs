// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using Humanizer;
using System.Collections.Generic;

namespace osu.Game.Online.API.Requests
{
    public abstract class GetUserBeatmapsRequest<T> : APIRequest<List<T>>
    {
        private readonly long userId;
        private readonly int offset;
        private readonly BeatmapSetType type;

        protected GetUserBeatmapsRequest(long userId, BeatmapSetType type, int offset = 0)
        {
            this.userId = userId;
            this.offset = offset;
            this.type = type;
        }

        protected override string Target => $@"users/{userId}/beatmapsets/{type.ToString().Underscore()}?offset={offset}";
    }

    public class GetUserBeatmapsRequest : GetUserBeatmapsRequest<GetBeatmapSetsResponse>
    {
        public GetUserBeatmapsRequest(long userID, BeatmapSetType type, int offset = 0)
            : base(userID, type, offset)
        {
            if(type == BeatmapSetType.MostPlayed)
                throw new ArgumentException("Please use " + nameof(GetUserMostPlayedBeatmapsRequest) + " instead");
        }
    }

    public enum BeatmapSetType
    {
        MostPlayed,
        Favourite,
        RankedAndApproved,
        Unranked,
        Graveyard
    }
}
