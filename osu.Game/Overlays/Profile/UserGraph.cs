// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Overlays.Profile
{
    /// <summary>
    /// Graph which is used in <see cref="UserProfileOverlay"/> to present changes in user statistics over time.
    /// </summary>
    /// <typeparam name="TKey">Type of data to be used for X-axis of the graph.</typeparam>
    /// <typeparam name="TValue">Type of data to be used for Y-axis of the graph.</typeparam>
    public abstract class UserGraph<TKey, TValue> : Container, IHasCustomTooltip<UserGraphTooltipContent>
    {
        protected const float FADE_DURATION = 150;

        private readonly UserLineGraph graph;
        private KeyValuePair<TKey, TValue>[] data;
        private int hoveredIndex = -1;

        protected UserGraph()
        {
            Add(graph = new UserLineGraph
            {
                RelativeSizeAxes = Axes.Both,
                Alpha = 0
            });

            graph.OnBallMove += i => hoveredIndex = i;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            graph.LineColour = colours.Yellow;
        }

        private float lastHoverPosition;

        protected override bool OnHover(HoverEvent e)
        {
            if (data?.Length > 1)
            {
                graph.UpdateBallPosition(lastHoverPosition = e.MousePosition.X);
                graph.ShowBar();

                return true;
            }

            return base.OnHover(e);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (data?.Length > 1)
                graph.UpdateBallPosition(e.MousePosition.X);

            return base.OnMouseMove(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            graph.HideBar();
            base.OnHoverLost(e);
        }

        /// <summary>
        /// Set of values which will be used to create a graph.
        /// </summary>
        [CanBeNull]
        protected KeyValuePair<TKey, TValue>[] Data
        {
            set
            {
                data = value;
                redrawGraph();
            }
        }

        private void redrawGraph()
        {
            hoveredIndex = -1;

            if (data?.Length > 1)
            {
                graph.DefaultValueCount = data.Length;
                graph.Values = data.Select(pair => GetDataPointHeight(pair.Value)).ToArray();
                ShowGraph();

                if (IsHovered)
                    graph.UpdateBallPosition(lastHoverPosition);
                return;
            }

            HideGraph();
        }

        /// <summary>
        /// Function used to convert <see cref="Data"/> point to it's Y-axis position on the graph.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        protected abstract float GetDataPointHeight(TValue value);

        protected virtual void ShowGraph() => graph.FadeIn(FADE_DURATION, Easing.Out);
        protected virtual void HideGraph() => graph.FadeOut(FADE_DURATION, Easing.Out);

        public ITooltip<UserGraphTooltipContent> GetCustomTooltip() => new UserGraphTooltip();

        public UserGraphTooltipContent TooltipContent
        {
            get
            {
                if (data == null || hoveredIndex == -1)
                    return null;

                var (key, value) = data[hoveredIndex];
                return GetTooltipContent(key, value);
            }
        }

        protected abstract UserGraphTooltipContent GetTooltipContent(TKey key, TValue value);

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
                            Width = 2,
                        },
                        movingBall = new CircularContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(20),
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

            private int calculateIndex(float mouseXPosition) => (int)Math.Clamp(MathF.Round(mouseXPosition / DrawWidth * (DefaultValueCount - 1)), 0, DefaultValueCount - 1);

            private Vector2 calculateBallPosition(int index)
            {
                float y = GetYPosition(Values.ElementAt(index));
                return new Vector2(index / (float)(DefaultValueCount - 1), y);
            }
        }

        private class UserGraphTooltip : VisibilityContainer, ITooltip<UserGraphTooltipContent>
        {
            protected readonly OsuSpriteText Label, Counter, BottomText;
            private readonly Box background;

            public UserGraphTooltip()
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
                                Spacing = new Vector2(3, 0),
                                Children = new Drawable[]
                                {
                                    Label = new OsuSpriteText
                                    {
                                        Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
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

            public void SetContent(UserGraphTooltipContent content)
            {
                Label.Text = content.Name;
                Counter.Text = content.Count;
                BottomText.Text = content.Time;
            }

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

    public class UserGraphTooltipContent
    {
        // todo: could use init-only properties on C# 9 which read better than a constructor.
        public LocalisableString Name { get; }
        public LocalisableString Count { get; }
        public LocalisableString Time { get; }

        public UserGraphTooltipContent(LocalisableString name, LocalisableString count, LocalisableString time)
        {
            Name = name;
            Count = count;
            Time = time;
        }
    }
}
