// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Overlays.Profile
{
    public abstract class UserGraph<TKey, TValue> : Container, IHasCustomTooltip
    {
        protected const float FADE_DURATION = 150;

        protected readonly UserLineGraph Graph;
        protected KeyValuePair<TKey, TValue>[] Data;
        protected int DataIndex;

        protected UserGraph()
        {
            Add(Graph = new UserLineGraph
            {
                RelativeSizeAxes = Axes.Both,
                Alpha = 0
            });

            Graph.OnBallMove += i => DataIndex = i;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Graph.LineColour = colours.Yellow;
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (Data?.Length > 1)
            {
                Graph.UpdateBallPosition(e.MousePosition.X);
                Graph.ShowBar();
            }

            return base.OnHover(e);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (Data?.Length > 1)
                Graph.UpdateBallPosition(e.MousePosition.X);

            return base.OnMouseMove(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (Data?.Length > 1)
                Graph.HideBar();

            base.OnHoverLost(e);
        }

        public ITooltip GetCustomTooltip() => GetTooltip();

        public object TooltipContent => GetTooltipContent();

        protected abstract UserGraphTooltip GetTooltip();

        protected abstract object GetTooltipContent();

        protected class UserLineGraph : LineGraph
        {
            private readonly CircularContainer movingBall;
            private readonly Container bar;
            private readonly Box ballBg;
            private readonly Box line;

            public Action<int> OnBallMove;

            public UserLineGraph()
            {
                Add(bar = new Container
                {
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Alpha = 0,
                    RelativePositionAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        line = new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Y,
                            Width = 1.5f,
                        },
                        movingBall = new CircularContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(18),
                            Masking = true,
                            BorderThickness = 4,
                            RelativePositionAxes = Axes.Y,
                            Child = ballBg = new Box { RelativeSizeAxes = Axes.Both }
                        }
                    }
                });
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider, OsuColour colours)
            {
                ballBg.Colour = colourProvider.Background5;
                movingBall.BorderColour = line.Colour = colours.Yellow;
            }

            public void UpdateBallPosition(float mouseXPosition)
            {
                const int duration = 200;
                int index = calculateIndex(mouseXPosition);
                Vector2 position = calculateBallPosition(index);
                movingBall.MoveToY(position.Y, duration, Easing.OutQuint);
                bar.MoveToX(position.X, duration, Easing.OutQuint);
                OnBallMove.Invoke(index);
            }

            public void ShowBar() => bar.FadeIn(FADE_DURATION);

            public void HideBar() => bar.FadeOut(FADE_DURATION);

            private int calculateIndex(float mouseXPosition) => (int)MathF.Round(mouseXPosition / DrawWidth * (DefaultValueCount - 1));

            private Vector2 calculateBallPosition(int index)
            {
                float y = GetYPosition(Values.ElementAt(index));
                return new Vector2(index / (float)(DefaultValueCount - 1), y);
            }
        }

        protected abstract class UserGraphTooltip : VisibilityContainer, ITooltip
        {
            protected readonly OsuSpriteText Counter, BottomText;
            private readonly Box background;

            protected UserGraphTooltip(string topText)
            {
                AutoSizeAxes = Axes.Both;
                Masking = true;
                CornerRadius = 10;

                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding(10),
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Children = new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                                        Text = $"{topText} "
                                    },
                                    Counter = new OsuSpriteText
                                    {
                                        Font = OsuFont.GetFont(size: 12, weight: FontWeight.Regular),
                                        Anchor = Anchor.BottomLeft,
                                        Origin = Anchor.BottomLeft,
                                    }
                                }
                            },
                            BottomText = new OsuSpriteText
                            {
                                Font = OsuFont.GetFont(size: 12, weight: FontWeight.Regular),
                            }
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                // Temporary colour since it's currently impossible to change it without bugs (see https://github.com/ppy/osu-framework/issues/3231)
                // If above is fixed, this should use OverlayColourProvider
                background.Colour = colours.Gray1;
            }

            public abstract bool SetContent(object content);

            private bool instantMove = true;

            public void Move(Vector2 pos)
            {
                if (instantMove)
                {
                    Position = pos;
                    instantMove = false;
                }
                else
                    this.MoveTo(pos, 200, Easing.OutQuint);
            }

            protected override void PopIn()
            {
                instantMove |= !IsPresent;
                this.FadeIn(200, Easing.OutQuint);
            }

            protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);
        }
    }
}
