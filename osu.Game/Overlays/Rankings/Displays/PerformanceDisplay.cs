// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Rulesets;
using osu.Framework.Graphics.Containers;
using osu.Framework.Allocation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using System.Threading;
using osu.Game.Overlays.Rankings.Tables;
using osu.Framework.Graphics.Shapes;
using System;
using osu.Game.Graphics;
using osuTK.Graphics;
using osu.Game.Users;

namespace osu.Game.Overlays.Rankings.Displays
{
    public class PerformanceDisplay : CompositeDrawable
    {
        public Action StartLoading;
        public Action FinishLoading;

        public readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();
        public readonly BindableWithCurrent<Country> Country = new BindableWithCurrent<Country>();
        private readonly Bindable<RankingsSortCriteria> sort = new Bindable<RankingsSortCriteria>();

        [Resolved]
        private IAPIProvider api { get; set; }

        private CancellationTokenSource cancellationToken;
        private GetUserRankingsRequest request;

        private Container content;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourProvider.Dark3
                                },
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
                        },
                        content = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Margin = new MarginPadding { Vertical = 20 },
                            Padding = new MarginPadding { Horizontal = UserProfileOverlay.CONTENT_X_MARGIN }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Country.BindValueChanged(_ => fetchRankings());
            sort.BindValueChanged(_ => fetchRankings());
            Ruleset.BindValueChanged(_ => fetchRankings(), true);
        }

        private void startLoading()
        {
            content.FadeColour(OsuColour.Gray(0.5f), 500, Easing.OutQuint);
            StartLoading?.Invoke();
        }

        private void finishLoading()
        {
            content.FadeColour(Color4.White, 500, Easing.OutQuint);
            FinishLoading?.Invoke();
        }

        private void fetchRankings()
        {
            startLoading();
            request?.Cancel();
            request = new GetUserRankingsRequest(Ruleset.Value, UserRankingsType.Performance, sort.Value, 1, Country.Value?.FlagName);
            request.Success += response => Schedule(() => createTable(response));
            api?.Queue(request);
        }

        private void createTable(GetUsersResponse response)
        {
            cancellationToken?.Cancel();
            var table = new ScoresTable(1, response.Users);
            LoadComponentAsync(table, loaded =>
            {
                content.Child = loaded;
                finishLoading();
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        protected override void Dispose(bool isDisposing)
        {
            request?.Cancel();
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
