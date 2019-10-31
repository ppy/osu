// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online.API.Requests
{
    public class GetUsersRequest : APIRequest<GetUsersResponse>
    {
        protected override string Target => @"rankings/osu/performance";
    }
}
