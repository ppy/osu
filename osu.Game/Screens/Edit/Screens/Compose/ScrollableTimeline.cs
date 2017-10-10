// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Screens.Compose
{
    public class ScrollableTimeline : CompositeDrawable
    {
        public readonly Bindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        private readonly ScrollingTimelineContainer timelineContainer;

        public ScrollableTimeline()
        {
            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex("111")
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            AutoSizeAxes = Axes.X,
                            RelativeSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = OsuColour.FromHex("222")
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    AutoSizeAxes = Axes.Y,
                                    Width = 160,
                                    Padding = new MarginPadding { Horizontal = 25 },
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 4),
                                    Children = new[]
                                    {
                                        new OsuCheckbox { LabelText = "Hit Objects" },
                                        new OsuCheckbox { LabelText = "Hit Sounds" },
                                        new OsuCheckbox { LabelText = "Waveform" }
                                    }
                                }
                            }
                        },
                        new Container
                        {
                            AutoSizeAxes = Axes.X,
                            RelativeSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = OsuColour.FromHex("333")
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    AutoSizeAxes = Axes.Both,
                                    Padding = new MarginPadding { Horizontal = 15 },
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 30),
                                    Children = new[]
                                    {
                                        new SpriteIcon
                                        {
                                            Size = new Vector2(18),
                                            Icon = FontAwesome.fa_search_plus,
                                            Colour = OsuColour.FromHex("555")
                                        },
                                        new SpriteIcon
                                        {
                                            Size = new Vector2(18),
                                            Icon = FontAwesome.fa_search_minus,
                                            Colour = OsuColour.FromHex("555")
                                        },
                                    }
                                }
                            }
                        },
                        timelineContainer = new ScrollingTimelineContainer { RelativeSizeAxes = Axes.Y }
                    }
                }
            };

            timelineContainer.Beatmap.BindTo(Beatmap);
        }

        protected override void Update()
        {
            base.Update();

            timelineContainer.Size = new Vector2(DrawSize.X - timelineContainer.DrawPosition.X, 1);
        }

        private class ScrollingTimelineContainer : ScrollContainer
        {
            public readonly Bindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

            private readonly BeatmapWaveformGraph graph;

            public ScrollingTimelineContainer()
                : base(Direction.Horizontal)
            {
                Masking = true;

                Add(graph = new BeatmapWaveformGraph
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex("222"),
                    Depth = float.MaxValue,
                });

                Content.AutoSizeAxes = Axes.None;
                Content.RelativeSizeAxes = Axes.Both;

                graph.Beatmap.BindTo(Beatmap);
            }

            private float minZoom = 1;
            public float MinZoom
            {
                get { return minZoom; }
                set
                {
                    if (value <= 0)
                        throw new ArgumentOutOfRangeException(nameof(value));
                    if (minZoom == value)
                        return;
                    minZoom = value;
                }
            }

            private float maxZoom = 30;
            public float MaxZoom
            {
                get { return maxZoom; }
                set
                {
                    if (value <= 0)
                        throw new ArgumentOutOfRangeException(nameof(value));
                    if (maxZoom == value)
                        return;
                    maxZoom = value;
                }
            }

            private float zoom = 1;
            public float Zoom
            {
                get { return zoom; }
                set
                {
                    value = MathHelper.Clamp(value, MinZoom, MaxZoom);
                    if (zoom == value)
                        return;
                    zoom = value;

                    Content.ResizeWidthTo(Zoom);
                }
            }

            protected override bool OnWheel(InputState state)
            {
                if (!state.Keyboard.ControlPressed)
                    return base.OnWheel(state);

                float relativeContentPosition = Content.ToLocalSpace(state.Mouse.NativeState.Position).X / Content.DrawSize.X;
                float position = ToLocalSpace(state.Mouse.NativeState.Position).X;

                Zoom += state.Mouse.WheelDelta;

                float scrollPos = Content.DrawSize.X * relativeContentPosition - position;
                ScrollTo(scrollPos, false);

                return true;
            }
        }
    }
}
