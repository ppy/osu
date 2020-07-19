// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Rankings.Tables;

namespace osu.Game.Overlays.Rankings.Displays
{
    public class CountryRankingsDisplay : RankingsDisplay<GetCountriesResponse>
    {
        protected override APIRequest<GetCountriesResponse> CreateRequest() => new GetCountryRankingsRequest(Current.Value);

        protected override Drawable CreateContent(GetCountriesResponse response) => new CountriesTable(1, response.Countries);
    }
}
