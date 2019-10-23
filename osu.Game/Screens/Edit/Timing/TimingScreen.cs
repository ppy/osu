// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Edit.Timing
{
    public class TimingScreen : EditorScreenWithTimeline
    {
        [Cached]
        private Bindable<IEnumerable<ControlPoint>> selectedPoints = new Bindable<IEnumerable<ControlPoint>>();

        [Resolved]
        private IAdjustableClock clock { get; set; }

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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedPoints.BindValueChanged(selected => { clock.Seek(selected.NewValue.First().Time); });
        }

        public class ControlPointList : CompositeDrawable
        {
            private OsuButton deleteButton;

            [Resolved]
            protected IBindable<WorkingBeatmap> Beatmap { get; private set; }

            [Resolved]
            private Bindable<IEnumerable<ControlPoint>> selectedPoints { get; set; }

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
                            ControlPoints = Beatmap.Value.Beatmap.ControlPointInfo.AllControlPoints
                        }
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        Direction = FillDirection.Horizontal,
                        Margin = new MarginPadding(10),
                        Spacing = new Vector2(5),
                        Children = new Drawable[]
                        {
                            deleteButton = new OsuButton
                            {
                                Text = "-",
                                Size = new Vector2(30, 30),
                                Action = delete,
                                Anchor = Anchor.BottomRight,
                                Origin = Anchor.BottomRight,
                            },
                            new OsuButton
                            {
                                Text = "+",
                                Action = addNew,
                                Size = new Vector2(30, 30),
                                Anchor = Anchor.BottomRight,
                                Origin = Anchor.BottomRight,
                            },
                        }
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                selectedPoints.BindValueChanged(selected => { deleteButton.Enabled.Value = selected.NewValue != null; }, true);
            }

            private void delete()
            {
            }

            private void addNew()
            {
            }
        }
    }
}
