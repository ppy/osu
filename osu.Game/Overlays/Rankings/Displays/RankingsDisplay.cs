// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Rulesets;
using osu.Framework.Graphics.Containers;
using osu.Framework.Allocation;
using System.Threading;
using System;
using osu.Game.Graphics;
using osuTK.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Overlays.Rankings.Displays
{
    public abstract class RankingsDisplay<T> : OverlayView<T>, IHasCurrentValue<RulesetInfo>
        where T : class
    {
        public Action StartLoading;
        public Action FinishLoading;

        private readonly BindableWithCurrent<RulesetInfo> current = new BindableWithCurrent<RulesetInfo>();

        public Bindable<RulesetInfo> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private CancellationTokenSource cancellationToken;

        private readonly Container content;
        private readonly Box headerBackground;

        protected RankingsDisplay()
        {
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
                        Children = new[]
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
                        Padding = new MarginPadding { Horizontal = RankingsOverlay.CONTENT_X_MARGIN }
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
            current.BindValueChanged(_ => PerformFetch());
        }

        protected override void PerformFetch()
        {
            startLoading();
            base.PerformFetch();
        }

        protected override void OnSuccess(T response)
        {
            LoadComponentAsync(CreateContent(response), loaded =>
            {
                content.Child = loaded;
                finishLoading();
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        protected virtual Drawable CreateHeader() => Empty();

        protected abstract Drawable CreateContent(T response);

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
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
