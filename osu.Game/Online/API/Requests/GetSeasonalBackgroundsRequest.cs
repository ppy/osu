// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetSeasonalBackgroundsRequest : APIRequest<APISeasonalBackgrounds>
    {
        protected override string Target => @"seasonal-backgrounds";
    }
}
