// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Edit.Timing
{
    public class TimingScreen : EditorScreenWithTimeline
    {
        [Cached]
        private readonly Bindable<ControlPoint> controlPoint = new Bindable<ControlPoint>();

        protected override Drawable CreateMainContent() => new GridContainer
        {
            RelativeSizeAxes = Axes.Both,
            ColumnDimensions = new[]
            {
                new Dimension(),
                new Dimension(GridSizeMode.Absolute, 200),
            },
            Content = new[]
            {
                new Drawable[]
                {
                    new ControlPointList(),
                    new ControlPointSettings(),
                },
            }
        };

        public class ControlPointList : CompositeDrawable
        {
            [Resolved]
            protected IBindable<WorkingBeatmap> Beatmap { get; private set; }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                RelativeSizeAxes = Axes.Both;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = colours.Gray0,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new OsuScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = new ControlPointTable
                        {
                            ControlPoints = Beatmap.Value.Beatmap.ControlPointInfo.TimingPoints
                        }
                    }
                };
            }
        }

        public class ControlPointSettings : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                RelativeSizeAxes = Axes.Both;

                InternalChild = new Box
                {
                    Colour = colours.Gray3,
                    RelativeSizeAxes = Axes.Both,
                };
            }
        }
    }

    public class ControlPointTable : TableContainer
    {
        private const float horizontal_inset = 20;
        private const float row_height = 25;
        private const int text_size = 14;

        private readonly FillFlowContainer backgroundFlow;

        [Resolved]
        private Bindable<ControlPoint> controlPoint { get; set; }

        public ControlPointTable()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding { Horizontal = horizontal_inset };
            RowSize = new Dimension(GridSizeMode.Absolute, row_height);

            AddInternal(backgroundFlow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Depth = 1f,
                Padding = new MarginPadding { Horizontal = -horizontal_inset },
                Margin = new MarginPadding { Top = row_height }
            });
        }

        public IReadOnlyList<ControlPoint> ControlPoints
        {
            set
            {
                Content = null;
                backgroundFlow.Clear();

                if (value?.Any() != true)
                    return;

                for (int i = 0; i < value.Count; i++)
                {
                    var cp = value[i];
                    backgroundFlow.Add(new RowBackground { Action = () => controlPoint.Value = cp });
                }

                Columns = createHeaders();
                Content = value.Select((s, i) => createContent(i, s)).ToArray().ToRectangular();
            }
        }

        private TableColumn[] createHeaders()
        {
            var columns = new List<TableColumn>
            {
                new TableColumn(string.Empty, Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
                new TableColumn("offset", Anchor.Centre),
                new TableColumn("BPM", Anchor.Centre),
                new TableColumn("Meter", Anchor.Centre),
                new TableColumn("Sample Set", Anchor.Centre),
                new TableColumn("Volume", Anchor.Centre),
            };

            return columns.ToArray();
        }

        private Drawable[] createContent(int index, ControlPoint controlPoint)
        {
            return new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = $"#{index + 1}",
                    Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Bold)
                },
                new OsuSpriteText
                {
                    Text = $"{controlPoint.Time}",
                    Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Bold)
                },
                new OsuSpriteText
                {
                    Text = $"{(controlPoint as TimingControlPoint)?.BeatLength.ToString(CultureInfo.InvariantCulture) ?? "-"}",
                    Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Bold)
                },
                new OsuSpriteText
                {
                    Text = $"{(controlPoint as TimingControlPoint)?.TimeSignature.ToString() ?? "-"}",
                    Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Bold)
                },
            };
        }

        protected override Drawable CreateHeader(int index, TableColumn column) => new HeaderText(column?.Header ?? string.Empty);

        private class HeaderText : OsuSpriteText
        {
            public HeaderText(string text)
            {
                Text = text.ToUpper();
                Font = OsuFont.GetFont(size: 12, weight: FontWeight.Black);
            }
        }

        public class RowBackground : OsuClickableContainer
        {
            private const int fade_duration = 100;

            private readonly Box hoveredBackground;

            public RowBackground()
            {
                RelativeSizeAxes = Axes.X;
                Height = 25;

                AlwaysPresent = true;

                CornerRadius = 3;
                Masking = true;

                Children = new Drawable[]
                {
                    hoveredBackground = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                hoveredBackground.Colour = colours.Blue;
            }

            protected override bool OnHover(HoverEvent e)
            {
                hoveredBackground.FadeIn(fade_duration, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                hoveredBackground.FadeOut(fade_duration, Easing.OutQuint);
                base.OnHoverLost(e);
            }
        }
    }
}
