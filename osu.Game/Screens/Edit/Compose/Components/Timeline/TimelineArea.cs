// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class TimelineArea : Container
    {
        private readonly Timeline timeline = new Timeline { RelativeSizeAxes = Axes.Both };

        protected override Container<Drawable> Content => timeline;

        [BackgroundDependencyLoader]
        private void load()
        {
            Masking = true;
            CornerRadius = 5;

            OsuCheckbox waveformCheckbox;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4Extensions.FromHex("111")
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Y,
                                AutoSizeAxes = Axes.X,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Color4Extensions.FromHex("222")
                                    },
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        AutoSizeAxes = Axes.Y,
                                        Width = 160,
                                        Padding = new MarginPadding { Horizontal = 15 },
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(0, 4),
                                        Children = new[]
                                        {
                                            waveformCheckbox = new OsuCheckbox { LabelText = "Waveform" }
                                        }
                                    }
                                }
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.Y,
                                AutoSizeAxes = Axes.X,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Color4Extensions.FromHex("333")
                                    },
                                    new Container<TimelineButton>
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        RelativeSizeAxes = Axes.Y,
                                        AutoSizeAxes = Axes.X,
                                        Masking = true,
                                        Children = new[]
                                        {
                                            new TimelineButton
                                            {
                                                RelativeSizeAxes = Axes.Y,
                                                Height = 0.5f,
                                                Icon = FontAwesome.Solid.SearchPlus,
                                                Action = () => changeZoom(1)
                                            },
                                            new TimelineButton
                                            {
                                                Anchor = Anchor.BottomLeft,
                                                Origin = Anchor.BottomLeft,
                                                RelativeSizeAxes = Axes.Y,
                                                Height = 0.5f,
                                                Icon = FontAwesome.Solid.SearchMinus,
                                                Action = () => changeZoom(-1)
                                            },
                                        }
                                    }
                                }
                            },
                            timeline
                        },
                    },
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.Distributed),
                    }
                }
            };

            waveformCheckbox.Current.Value = true;

            timeline.WaveformVisible.BindTo(waveformCheckbox.Current);
        }

        private void changeZoom(float change) => timeline.Zoom += change;
    }
}
