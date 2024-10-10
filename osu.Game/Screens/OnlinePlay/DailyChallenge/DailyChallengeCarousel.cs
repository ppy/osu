// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.DailyChallenge
{
    public partial class DailyChallengeCarousel : Container
    {
        private const int switch_interval = 20_500;

        private readonly Container content;
        private readonly FillFlowContainer<NavigationDot> navigationFlow;

        protected override Container<Drawable> Content => content;

        private double clockStartTime;
        private int lastDisplayed = -1;

        public DailyChallengeCarousel()
        {
            InternalChildren = new Drawable[]
            {
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Bottom = 40 },
                },
                navigationFlow = new FillFlowContainer<NavigationDot>
                {
                    AutoSizeAxes = Axes.X,
                    Height = 15,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Spacing = new Vector2(10),
                }
            };
        }

        public override void Add(Drawable drawable)
        {
            drawable.RelativeSizeAxes = Axes.Both;
            drawable.Size = Vector2.One;
            drawable.Alpha = 0;

            base.Add(drawable);

            navigationFlow.Add(new NavigationDot { Clicked = onManualNavigation });
        }

        public override bool Remove(Drawable drawable, bool disposeImmediately)
        {
            int index = content.IndexOf(drawable);

            if (index > 0)
                navigationFlow.Remove(navigationFlow[index], true);

            return base.Remove(drawable, disposeImmediately);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            clockStartTime = Clock.CurrentTime;
        }

        protected override void Update()
        {
            base.Update();

            if (content.Count == 0)
            {
                lastDisplayed = -1;
                return;
            }

            double elapsed = Clock.CurrentTime - clockStartTime;

            int currentDisplay = (int)(elapsed / switch_interval) % content.Count;
            double displayProgress = (elapsed % switch_interval) / switch_interval;

            navigationFlow[currentDisplay].Active.Value = true;

            if (content.Count > 1)
                navigationFlow[currentDisplay].Progress = (float)displayProgress;

            if (currentDisplay == lastDisplayed)
                return;

            if (lastDisplayed >= 0)
            {
                content[lastDisplayed].FadeOutFromOne(250, Easing.OutQuint);
                navigationFlow[lastDisplayed].Active.Value = false;
            }

            content[currentDisplay].Delay(250).Then().FadeInFromZero(250, Easing.OutQuint);

            lastDisplayed = currentDisplay;
        }

        private void onManualNavigation(NavigationDot dot)
        {
            int index = navigationFlow.IndexOf(dot);

            if (index < 0)
                return;

            clockStartTime = Clock.CurrentTime - index * switch_interval;
        }

        private partial class NavigationDot : CompositeDrawable
        {
            public required Action<NavigationDot> Clicked { get; init; }

            public BindableBool Active { get; } = new BindableBool();

            private double progress;

            public float Progress
            {
                set
                {
                    if (progress == value)
                        return;

                    progress = value;
                    progressLayer.Width = value;
                }
            }

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            private Box background = null!;
            private Box progressLayer = null!;
            private Box hoverLayer = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                Size = new Vector2(15);

                InternalChildren = new Drawable[]
                {
                    new CircularContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            background = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colourProvider.Light4,
                            },
                            progressLayer = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0,
                                Colour = colourProvider.Highlight1,
                                Blending = BlendingParameters.Additive,
                                Alpha = 0,
                            },
                            hoverLayer = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Colour4.White,
                                Blending = BlendingParameters.Additive,
                                Alpha = 0,
                            }
                        }
                    },
                    new HoverClickSounds()
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Active.BindValueChanged(val =>
                {
                    if (val.NewValue)
                    {
                        background.FadeColour(colourProvider.Highlight1, 250, Easing.OutQuint);
                        this.ResizeWidthTo(30, 250, Easing.OutQuint);
                        progressLayer.Width = 0;
                        progressLayer.Alpha = 0.5f;
                    }
                    else
                    {
                        background.FadeColour(colourProvider.Light4, 250, Easing.OutQuint);
                        this.ResizeWidthTo(15, 250, Easing.OutQuint);
                        progressLayer.FadeOut(250, Easing.OutQuint);
                    }
                }, true);
            }

            protected override bool OnHover(HoverEvent e)
            {
                base.OnHover(e);
                hoverLayer.FadeTo(0.2f, 250, Easing.OutQuint);
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                hoverLayer.FadeOut(250, Easing.OutQuint);
                base.OnHoverLost(e);
            }

            protected override bool OnClick(ClickEvent e)
            {
                Clicked(this);

                hoverLayer.FadeTo(1)
                          .Then().FadeTo(IsHovered ? 0.2f : 0, 250, Easing.OutQuint);

                return true;
            }
        }
    }
}
