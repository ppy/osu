// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests
{
    public abstract class GetRankingsRequest<TModel> : APIRequest<TModel> where TModel : class
    {
        private readonly RulesetInfo ruleset;
        private readonly int page;

        protected GetRankingsRequest(RulesetInfo ruleset, int page = 1)
        {
            this.ruleset = ruleset;
            this.page = page;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.AddParameter("page", page.ToString());

            return req;
        }

        protected override string Target => $"rankings/{ruleset.ShortName}/{TargetPostfix()}";

        protected abstract string TargetPostfix();
    }
}
