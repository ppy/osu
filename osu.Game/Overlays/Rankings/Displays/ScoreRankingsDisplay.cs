// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Rankings.Tables;
using osu.Game.Online.API;

namespace osu.Game.Overlays.Rankings.Displays
{
    public class ScoreRankingsDisplay : RankingsDisplay<GetUsersResponse>
    {
        private readonly Bindable<RankingsSortCriteria> sort = new Bindable<RankingsSortCriteria>();

        protected override void LoadComplete()
        {
            base.LoadComplete();
            sort.BindValueChanged(_ => PerformFetch());
        }

        protected override APIRequest<GetUsersResponse> CreateRequest() => new GetUserRankingsRequest(Current.Value, UserRankingsType.Score, sort.Value);

        protected override Drawable CreateHeader() => new RankingsSortTabControl
        {
            Margin = new MarginPadding { Vertical = 20 },
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            Current = sort
        };

        protected override Drawable CreateContent(GetUsersResponse response) => new ScoresTable(1, response.Users);
    }
}
