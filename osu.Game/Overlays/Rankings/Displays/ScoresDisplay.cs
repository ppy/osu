// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Rankings.Tables;
using osu.Game.Online.API;

namespace osu.Game.Overlays.Rankings.Displays
{
    public class ScoresDisplay : RankingsDisplay
    {
        private readonly Bindable<RankingsSortCriteria> sort = new Bindable<RankingsSortCriteria>();

        protected override void LoadComplete()
        {
            base.LoadComplete();
            sort.BindValueChanged(_ => FetchRankings(), true);
        }

        protected override APIRequest CreateRequest() => new GetUserRankingsRequest(Current.Value, UserRankingsType.Score, sort.Value);

        protected override Drawable CreateHeader() => new RankingsSortTabControl
        {
            Margin = new MarginPadding { Vertical = 20, Right = UserProfileOverlay.CONTENT_X_MARGIN },
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            Current = sort
        };

        protected override Drawable CreateContent(APIRequest request) => new ScoresTable(1, ((GetUserRankingsRequest)request).Result.Users);
    }
}
