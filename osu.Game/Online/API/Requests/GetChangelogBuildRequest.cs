// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetChangelogBuildRequest : APIRequest<APIChangelogBuild>
    {
        private readonly string name;
        private readonly string version;

        public GetChangelogBuildRequest(string streamName, string buildVersion)
        {
            name = streamName;
            version = buildVersion;
        }

        protected override string Target => $@"changelog/{name}/{version}";
    }
}
