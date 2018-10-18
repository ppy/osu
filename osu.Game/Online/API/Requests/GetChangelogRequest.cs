// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetChangelogRequest : APIRequest<APIChangelogBuild[]>
    {
        protected override string Target => @"changelog";
        protected override string Uri => $@"https://houtarouoreki.github.io/fake-api/{Target}/index"; // for testing
    }
}
