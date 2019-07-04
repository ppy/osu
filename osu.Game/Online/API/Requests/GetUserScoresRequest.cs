// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests
{
    public class GetUserScoresRequest : APIRequest<List<APILegacyScoreInfo>>
    {
        private readonly long userId;
        private readonly ScoreType type;
        private readonly int offset;
        private readonly RulesetInfo ruleset;

        public GetUserScoresRequest(long userId, ScoreType type, int offset = 0, RulesetInfo ruleset = null)
        {
            this.userId = userId;
            this.type = type;
            this.offset = offset;
            this.ruleset = ruleset;
        }

        // ReSharper disable once ImpureMethodCallOnReadonlyValueField
        protected override string Target => $@"users/{userId}/scores/{type.ToString().ToLowerInvariant()}?offset={offset}{(ruleset != null ? "&mode=" + ruleset.ShortName : "")}";
    }

    public enum ScoreType
    {
        Best,
        Firsts,
        Recent
    }
}
