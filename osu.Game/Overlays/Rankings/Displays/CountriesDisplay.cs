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
using System;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Overlays.Rankings.Displays
{
    public class CountriesDisplay : CompositeDrawable
    {
        public Action StartLoading;
        public Action FinishLoading;

        public readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

        [Resolved]
        private IAPIProvider api { get; set; }

        private CancellationTokenSource cancellationToken;
        private GetCountryRankingsRequest request;

        private Container content;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChild = content = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Margin = new MarginPadding { Vertical = 20 },
                Padding = new MarginPadding { Horizontal = UserProfileOverlay.CONTENT_X_MARGIN }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
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
            request = new GetCountryRankingsRequest(Ruleset.Value);
            request.Success += response => Schedule(() => createTable(response));
            api?.Queue(request);
        }

        private void createTable(GetCountriesResponse response)
        {
            cancellationToken?.Cancel();
            var table = new CountriesTable(1, response.Countries);
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
