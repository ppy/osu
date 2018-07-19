// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetChangelogRequest : APIRequest<APIChangelog[]>
    {
        protected override string Uri => Target;
        protected override string Target => "https://api.myjson.com/bins/6zv2i";
    }
}
