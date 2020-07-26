// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Rankings.Tables;
using osu.Game.Users;
using osu.Game.Online.API;

namespace osu.Game.Overlays.Rankings.Displays
{
    public class PerformanceRankingsDisplay : RankingsDisplay<GetUsersResponse>
    {
        public readonly BindableWithCurrent<Country> Country = new BindableWithCurrent<Country>();
        private readonly Bindable<RankingsSortCriteria> sort = new Bindable<RankingsSortCriteria>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Country.BindValueChanged(_ => PerformFetch());
            sort.BindValueChanged(_ => PerformFetch());
        }

        protected override APIRequest<GetUsersResponse> CreateRequest() => new GetUserRankingsRequest(Current.Value, UserRankingsType.Performance, sort.Value, 1, Country.Value?.FlagName);

        protected override Drawable CreateHeader() => new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Children = new Drawable[]
            {
                new CountryFilter
                {
                    Current = Country
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Child = new RankingsSortTabControl
                    {
                        Margin = new MarginPadding { Vertical = 20 },
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Current = sort
                    }
                }
            }
        };

        protected override Drawable CreateContent(GetUsersResponse response) => new PerformanceTable(1, response.Users);
    }
}
