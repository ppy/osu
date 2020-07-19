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
    public class PerformanceRankingsDisplay : RankingsDisplay
    {
        public readonly BindableWithCurrent<Country> Country = new BindableWithCurrent<Country>();
        private readonly Bindable<RankingsSortCriteria> sort = new Bindable<RankingsSortCriteria>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Country.BindValueChanged(_ => FetchRankings());
            sort.BindValueChanged(_ => FetchRankings(), true);
        }

        protected override APIRequest CreateRequest() => new GetUserRankingsRequest(Current.Value, UserRankingsType.Performance, sort.Value, 1, Country.Value?.FlagName);

        protected override Drawable CreateHeader() => new Container
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Children = new Drawable[]
            {
                new FillFlowContainer
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
                                Margin = new MarginPadding { Vertical = 20, Right = UserProfileOverlay.CONTENT_X_MARGIN },
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Current = sort
                            }
                        }
                    }
                }
            }
        };

        protected override Drawable CreateContent(APIRequest request) => new PerformanceTable(1, ((GetUserRankingsRequest)request).Result.Users);
    }
}
