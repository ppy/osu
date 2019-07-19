// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetUserScoresRequest : PaginatedAPIRequest<List<APILegacyScoreInfo>>
    {
        private readonly long userId;
        private readonly ScoreType type;

        public GetUserScoresRequest(long userId, ScoreType type, int page = 0, int itemsPerPage = 5)
            : base(page, itemsPerPage)
        {
            this.userId = userId;
            this.type = type;
        }

        protected override string Target => $@"users/{userId}/scores/{type.ToString().ToLowerInvariant()}";
    }

    public enum ScoreType
    {
        Best,
        Firsts,
        Recent
    }
}
