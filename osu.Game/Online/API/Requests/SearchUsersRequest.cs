// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;

namespace osu.Game.Online.API.Requests
{
    /// <summary>
    /// Lookup up users with the given <see cref="Query"/>.
    /// </summary>
    public class SearchUsersRequest : APIRequest<SearchUsersResponse>
    {
        public readonly string Query;

        public SearchUsersRequest(string query)
        {
            Query = query;
        }

        protected override string Target => "search";

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.AddParameter("mode", "user");
            req.AddParameter("query", Query);

            return req;
        }
    }
}
