// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Rulesets;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using osu.Framework.Allocation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Rankings.Tables;
using System.Linq;
using System.Threading;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.BeatmapListing.Panels;

namespace osu.Game.Overlays.Rankings
{
    public class SpotlightsLayout : CompositeDrawable
    {
        public readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

        private readonly Bindable<APISpotlight> selectedSpotlight = new Bindable<APISpotlight>();
        private readonly Bindable<RankingsSortCriteria> sort = new Bindable<RankingsSortCriteria>();

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        private CancellationTokenSource cancellationToken;
        private GetSpotlightRankingsRequest getRankingsRequest;
        private GetSpotlightsRequest spotlightsRequest;

        private SpotlightSelector selector;
        private Container content;
        private LoadingLayer loading;

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
                    selector = new SpotlightSelector
                    {
                        Current = selectedSpotlight,
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

            sort.BindTo(selector.Sort);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedSpotlight.BindValueChanged(_ => onSpotlightChanged());
            sort.BindValueChanged(_ => onSpotlightChanged());
            Ruleset.BindValueChanged(onRulesetChanged);

            getSpotlights();
        }

        private void getSpotlights()
        {
            spotlightsRequest = new GetSpotlightsRequest();
            spotlightsRequest.Success += response => Schedule(() => selector.Spotlights = response.Spotlights);
            api.Queue(spotlightsRequest);
        }

        private void onRulesetChanged(ValueChangedEvent<RulesetInfo> ruleset)
        {
            if (!selector.Spotlights.Any())
                return;

            selectedSpotlight.TriggerChange();
        }

        private void onSpotlightChanged()
        {
            loading.Show();

            cancellationToken?.Cancel();
            getRankingsRequest?.Cancel();

            getRankingsRequest = new GetSpotlightRankingsRequest(Ruleset.Value, selectedSpotlight.Value.Id, sort.Value);
            getRankingsRequest.Success += onSuccess;
            api.Queue(getRankingsRequest);
        }

        private void onSuccess(GetSpotlightRankingsResponse response)
        {
            LoadComponentAsync(createContent(response), loaded =>
            {
                selector.ShowInfo(response);

                content.Clear();
                content.Add(loaded);

                loading.Hide();
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        private Drawable createContent(GetSpotlightRankingsResponse response) => new FillFlowContainer
        {
            AutoSizeAxes = Axes.Y,
            RelativeSizeAxes = Axes.X,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(0, 20),
            Children = new Drawable[]
            {
                new ScoresTable(1, response.Users),
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Spacing = new Vector2(10),
                    Children = response.BeatmapSets.Select(b => new GridBeatmapPanel(b)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                    }).ToList()
                }
            }
        };

        protected override void Dispose(bool isDisposing)
        {
            spotlightsRequest?.Cancel();
            getRankingsRequest?.Cancel();
            cancellationToken?.Cancel();

            base.Dispose(isDisposing);
        }
    }
}
