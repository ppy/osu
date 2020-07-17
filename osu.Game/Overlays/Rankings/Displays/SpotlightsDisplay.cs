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
using System.Collections.Generic;

namespace osu.Game.Overlays.Rankings.Displays
{
    public class SpotlightsDisplay : RankingsDisplay
    {
        private readonly Bindable<APISpotlight> selectedSpotlight = new Bindable<APISpotlight>();
        private Bindable<RankingsSortCriteria> sort => selector.Sort;

        [Resolved]
        private RulesetStore rulesets { get; set; }

        public IEnumerable<APISpotlight> Spotlights
        {
            get => selector.Spotlights;
            set => selector.Spotlights = value;
        }

        private SpotlightSelector selector;

        protected override void LoadComplete()
        {
            base.LoadComplete();
            selectedSpotlight.BindValueChanged(_ => FetchRankings());
            sort.BindValueChanged(_ => FetchRankings(), true);
        }

        protected override APIRequest CreateRequest() => new GetSpotlightRankingsRequest(Current.Value, selectedSpotlight.Value.Id, sort.Value);

        protected override Drawable CreateHeader() => selector = new SpotlightSelector
        {
            Current = selectedSpotlight
        };

        protected override Drawable CreateContent(APIRequest request)
        {
            var response = ((GetSpotlightRankingsRequest)request).Result;
            selector.ShowInfo(response);

            return new FillFlowContainer
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
        }
    }
}
