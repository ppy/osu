// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetChangelogBuildRequest : APIRequest<APIChangelog>
    {
        private readonly string name;
        private readonly string version;

        public GetChangelogBuildRequest(string streamName, string buildVersion)
        {
            name = streamName;
            version = buildVersion;
        }

        //protected override string Target => $@"changelog/{name}/{version}";
        protected override string Uri => @"https://api.myjson.com/bins/ya5q2"; // for testing
    }
}
