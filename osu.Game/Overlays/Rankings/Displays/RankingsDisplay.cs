// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Rulesets;
using osu.Framework.Graphics.Containers;
using osu.Framework.Allocation;
using osu.Game.Online.API;
using System.Threading;
using System;
using osu.Game.Graphics;
using osuTK.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Overlays.Rankings.Displays
{
    public abstract class RankingsDisplay : CompositeDrawable, IHasCurrentValue<RulesetInfo>
    {
        public Action StartLoading;
        public Action FinishLoading;

        private readonly BindableWithCurrent<RulesetInfo> current = new BindableWithCurrent<RulesetInfo>();

        public Bindable<RulesetInfo> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        [Resolved]
        private IAPIProvider api { get; set; }

        private CancellationTokenSource cancellationToken;
        private APIRequest request;

        private readonly Container content;
        private readonly Box headerBackground;

        public RankingsDisplay()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChild = new FillFlowContainer
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
                        Depth = -float.MaxValue,
                        Children = new Drawable[]
                        {
                            headerBackground = new Box
                            {
                                RelativeSizeAxes = Axes.Both
                            },
                            CreateHeader()
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
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            headerBackground.Colour = colourProvider.Dark3;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            current.BindValueChanged(_ => FetchRankings());
        }

        protected void FetchRankings()
        {
            startLoading();

            cancellationToken?.Cancel();
            request?.Cancel();

            request = CreateRequest();
            request.Success += () => Schedule(() => createContent(request));
            api?.Queue(request);
        }

        private void createContent(APIRequest request)
        {
            LoadComponentAsync(CreateContent(request), loaded =>
            {
                content.Child = loaded;
                finishLoading();
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        protected virtual Drawable CreateHeader() => Empty();

        protected abstract APIRequest CreateRequest();

        protected abstract Drawable CreateContent(APIRequest request);

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

        protected override void Dispose(bool isDisposing)
        {
            request?.Cancel();
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
