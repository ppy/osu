// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
