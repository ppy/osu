// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Online.API.Requests.Responses;
using System.Collections.Generic;

namespace osu.Game.Online.API.Requests
{
    public class GetChangelogLatestBuildsRequest : APIRequest<List<APIChangelogBuild>>
    {
        protected override string Target => @"changelog/latest-builds";
        protected override string Uri => $@"https://houtarouoreki.github.io/fake-api/{Target}"; // for testing
    }
}
