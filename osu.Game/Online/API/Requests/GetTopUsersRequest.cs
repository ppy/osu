// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

namespace osu.Game.Online.API.Requests
{
    public class GetTopUsersRequest : APIRequest<GetTopUsersResponse>
    {
        protected override string Target => @"rankings/osu/performance";
    }
}
