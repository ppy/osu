// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Rankings.Tables;

namespace osu.Game.Overlays.Rankings.Displays
{
    public class CountriesDisplay : RankingsDisplay
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();
            FetchRankings();
        }

        protected override APIRequest CreateRequest() => new GetCountryRankingsRequest(Current.Value);

        protected override Drawable CreateContent(APIRequest request) => new CountriesTable(1, ((GetCountryRankingsRequest)request).Result.Countries);
    }
}
