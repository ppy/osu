// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Rankings.Tables;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Rankings
{
    public partial class MatchmakingLayout : CompositeDrawable
    {
        public readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

        private readonly Bindable<APIMatchmakingPool> selectedPool = new Bindable<APIMatchmakingPool>();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private CancellationTokenSource? cancellationToken;
        private GetMatchmakingPoolsRequest? getPoolsRequest;
        private GetMatchmakingRankingRequest? getRankingRequest;

        private MatchmakingPoolSelector selector = null!;
        private Container content = null!;
        private LoadingLayer loading = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new ReverseChildIDFillFlowContainer<Drawable>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    selector = new MatchmakingPoolSelector
                    {
                        Current = selectedPool,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            content = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Margin = new MarginPadding { Vertical = 10 }
                            },
                            loading = new LoadingLayer(true)
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedPool.BindValueChanged(_ => onPoolChanged());
            Ruleset.BindValueChanged(_ => onRulesetChanged());

            getMatchmakingPools();
        }

        private void getMatchmakingPools()
        {
            getPoolsRequest?.Cancel();

            getPoolsRequest = new GetMatchmakingPoolsRequest(Ruleset.Value);
            getPoolsRequest.Success += response => Schedule(() => selector.Pools = response);
            api.Queue(getPoolsRequest);
        }

        private void onRulesetChanged()
        {
            if (!selector.Pools.Any())
                return;

            selectedPool.TriggerChange();
        }

        private void onPoolChanged()
        {
            loading.Show();

            cancellationToken?.Cancel();
            getRankingRequest?.Cancel();

            getRankingRequest = new GetMatchmakingRankingRequest(Ruleset.Value, selectedPool.Value);
            getRankingRequest.Success += onSuccess;

            api.Queue(getRankingRequest);
        }

        private void onSuccess(GetMatchmakingRankingResponse response)
        {
            LoadComponentAsync(new MatchmakingTable(1, response.Users), loaded =>
            {
                content.Clear();
                content.Add(loaded);

                loading.Hide();
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        protected override void Dispose(bool isDisposing)
        {
            getPoolsRequest?.Cancel();
            getRankingRequest?.Cancel();
            cancellationToken?.Cancel();

            base.Dispose(isDisposing);
        }
    }
}
