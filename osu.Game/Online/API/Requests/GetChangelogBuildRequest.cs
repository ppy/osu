// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetChangelogBuildRequest : APIRequest<APIChangelog>
    {
        private string url;
        /// <param name="url">This will need to be changed to "long Id"
        /// Placeholder for testing</param>
        GetChangelogBuildRequest(string url)
        {
            this.url = url;
        }
        protected override string Uri => @"";
        protected override string Target => url;
    }
}
