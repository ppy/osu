// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetUserScoresRequest : APIRequest<List<APILegacyScoreInfo>>
    {
        private readonly long userId;
        private readonly ScoreType type;
        private readonly int offset;
        private readonly int limit;

        public GetUserScoresRequest(long userId, ScoreType type, int offset = 0, int limit = 5)
        {
            this.userId = userId;
            this.type = type;
            this.offset = offset;
            this.limit = limit;
        }

        // ReSharper disable once ImpureMethodCallOnReadonlyValueField
        protected override string Target => $@"users/{userId}/scores/{type.ToString().ToLowerInvariant()}?offset={offset}&limit={limit}";
    }

    public enum ScoreType
    {
        Best,
        Firsts,
        Recent
    }
}
