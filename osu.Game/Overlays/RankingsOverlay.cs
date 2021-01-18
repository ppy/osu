// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Overlays.Rankings;
using osu.Game.Users;
using osu.Game.Rulesets;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Rankings.Tables;

namespace osu.Game.Overlays
{
    public class RankingsOverlay : TabbableOnlineOverlay<RankingsOverlayHeader, RankingsScope>
    {
        protected Bindable<Country> Country => Header.Country;

        protected Bindable<RankingsScope> Scope => Header.Current;

        private APIRequest lastRequest;

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private Bindable<RulesetInfo> ruleset { get; set; }

        public RankingsOverlay()
            : base(OverlayColourScheme.Green)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Header.Ruleset.BindTo(ruleset);

            Country.BindValueChanged(_ =>
            {
                // if a country is requested, force performance scope.
                if (Country.Value != null)
                    Scope.Value = RankingsScope.Performance;

                Scheduler.AddOnce(loadNewContent);
            });

            // Unbind events from scope so base class event will not be called
            Scope.UnbindEvents();
            Scope.BindValueChanged(_ =>
            {
                // country filtering is only valid for performance scope.
                if (Scope.Value != RankingsScope.Performance)
                    Country.Value = null;

                Scheduler.AddOnce(loadNewContent);
            });

            ruleset.BindValueChanged(_ =>
            {
                if (Scope.Value == RankingsScope.Spotlights)
                    return;

                Scheduler.AddOnce(loadNewContent);
            });
        }

        protected override RankingsOverlayHeader CreateHeader() => new RankingsOverlayHeader();

        public void ShowCountry(Country requested)
        {
            if (requested == null)
                return;

            Show();

            Country.Value = requested;
        }

        public void ShowSpotlights()
        {
            Scope.Value = RankingsScope.Spotlights;
            Show();
        }

        protected override void CreateDisplayToLoad(RankingsScope tab)
        {
            lastRequest?.Cancel();

            if (Scope.Value == RankingsScope.Spotlights)
            {
                LoadDisplay(new SpotlightsLayout
                {
                    Ruleset = { BindTarget = ruleset }
                });
                return;
            }

            var request = createScopedRequest();
            lastRequest = request;

            if (request == null)
            {
                LoadDisplay(Empty());
                return;
            }

            request.Success += () => Schedule(() => LoadDisplay(createTableFromResponse(request)));
            request.Failure += _ => Schedule(() => LoadDisplay(Empty()));

            api.Queue(request);
        }

        private APIRequest createScopedRequest()
        {
            switch (Scope.Value)
            {
                case RankingsScope.Performance:
                    return new GetUserRankingsRequest(ruleset.Value, country: Country.Value?.FlagName);

                case RankingsScope.Country:
                    return new GetCountryRankingsRequest(ruleset.Value);

                case RankingsScope.Score:
                    return new GetUserRankingsRequest(ruleset.Value, UserRankingsType.Score);
            }

            return null;
        }

        private Drawable createTableFromResponse(APIRequest request)
        {
            switch (request)
            {
                case GetUserRankingsRequest userRequest:
                    switch (userRequest.Type)
                    {
                        case UserRankingsType.Performance:
                            return new PerformanceTable(1, userRequest.Result.Users);

                        case UserRankingsType.Score:
                            return new ScoresTable(1, userRequest.Result.Users);
                    }

                    return null;

                case GetCountryRankingsRequest countryRequest:
                    return new CountriesTable(1, countryRequest.Result.Countries);
            }

            return null;
        }

        private void loadNewContent() => OnTabChanged(Scope.Value);

        protected override void Dispose(bool isDisposing)
        {
            lastRequest?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
