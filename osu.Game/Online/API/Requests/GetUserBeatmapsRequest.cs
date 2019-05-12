// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using System.Collections.Generic;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetUserBeatmapsRequest : APIRequest<List<APIBeatmapSet>>
    {
        private readonly long userId;
        private readonly int offset;
        private readonly BeatmapSetType type;

        public GetUserBeatmapsRequest(long userId, BeatmapSetType type, int offset = 0)
        {
            this.userId = userId;
            this.offset = offset;
            this.type = type;
        }

        // ReSharper disable once ImpureMethodCallOnReadonlyValueField
        protected override string Target => $@"users/{userId}/beatmapsets/{type.ToString().Underscore()}?offset={offset}";
    }

    public enum BeatmapSetType
    {
        Favourite,
        RankedAndApproved,
        Unranked,
        Graveyard
    }
}
