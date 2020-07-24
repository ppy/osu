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
using osu.Game.Overlays.BeatmapListing.Panels;
using System.Threading;

namespace osu.Game.Overlays.Rankings.Displays
{
    public class SpotlightsRankingsDisplay : RankingsDisplay<SpotlightsCollection>
    {
        private readonly Bindable<APISpotlight> selectedSpotlight = new Bindable<APISpotlight>();
        private readonly Bindable<RankingsSortCriteria> sort = new Bindable<RankingsSortCriteria>();

        [Resolved]
        private RulesetStore rulesets { get; set; }

        private SpotlightSelector selector;

        protected override bool CreateContentOnSucess => false;

        protected override APIRequest<SpotlightsCollection> CreateRequest() => new GetSpotlightsRequest();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedSpotlight.BindValueChanged(_ => performSpotlightFetch());
            sort.BindValueChanged(_ => performSpotlightFetch());
        }

        protected override void OnSuccess(SpotlightsCollection response)
        {
            base.OnSuccess(response);
            selector.Spotlights = response.Spotlights;
        }

        protected override Drawable CreateHeader() => selector = new SpotlightSelector
        {
            Current = selectedSpotlight,
            Sort = { BindTarget = sort }
        };

        private GetSpotlightRankingsRequest getSpotlightRankingsRequest;
        private CancellationTokenSource cancellationToken;

        private void performSpotlightFetch()
        {
            InvokeStartLoading();
            getSpotlightRankingsRequest?.Cancel();
            cancellationToken?.Cancel();

            getSpotlightRankingsRequest = new GetSpotlightRankingsRequest(Current.Value, selectedSpotlight.Value.Id, sort.Value);
            getSpotlightRankingsRequest.Success += response => Schedule(() => loadNewTable(response));
            API.Queue(getSpotlightRankingsRequest);
        }

        private void loadNewTable(GetSpotlightRankingsResponse response)
        {
            LoadComponentAsync(createTable(response), loaded =>
            {
                selector.ShowInfo(response);
                Content.Child = loaded;
                InvokeFinishLoading();
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        private Drawable createTable(GetSpotlightRankingsResponse response) => new FillFlowContainer
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
                    Children = response.BeatmapSets.Select(b => new GridBeatmapPanel(b.ToBeatmapSet(rulesets))
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                    }).ToList()
                }
            }
        };

        protected override void Dispose(bool isDisposing)
        {
            getSpotlightRankingsRequest?.Cancel();
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
