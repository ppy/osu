// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Edit.Timing.RowAttributes;
using osuTK;

namespace osu.Game.Screens.Edit.Timing
{
    public partial class ControlPointTable : EditorTable
    {
        [Resolved]
        private Bindable<ControlPointGroup> selectedGroup { get; set; } = null!;

        [Resolved]
        private EditorClock clock { get; set; } = null!;

        public const float TIMING_COLUMN_WIDTH = 300;

        public IEnumerable<ControlPointGroup> ControlGroups
        {
            set
            {
                Content = null;
                BackgroundFlow.Clear();

                if (!value.Any())
                    return;

                foreach (var group in value)
                {
                    BackgroundFlow.Add(new RowBackground(group)
                    {
                        Action = () =>
                        {
                            selectedGroup.Value = group;
                            clock.SeekSmoothlyTo(group.Time);
                        }
                    });
                }

                Columns = createHeaders();
                Content = value.Select(createContent).ToArray().ToRectangular();

                updateSelectedGroup();
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedGroup.BindValueChanged(_ => updateSelectedGroup(), true);
        }

        private void updateSelectedGroup() => SetSelectedRow(selectedGroup.Value);

        private TableColumn[] createHeaders()
        {
            var columns = new List<TableColumn>
            {
                new TableColumn("Time", Anchor.CentreLeft, new Dimension(GridSizeMode.Absolute, TIMING_COLUMN_WIDTH)),
                new TableColumn("Attributes", Anchor.CentreLeft),
            };

            return columns.ToArray();
        }

        private Drawable[] createContent(ControlPointGroup group)
        {
            return new Drawable[]
            {
                new ControlGroupTiming(group),
                new ControlGroupAttributes(group, c => c is not TimingControlPoint)
            };
        }

        private partial class ControlGroupTiming : FillFlowContainer
        {
            public ControlGroupTiming(ControlPointGroup group)
            {
                Name = @"ControlGroupTiming";
                RelativeSizeAxes = Axes.Y;
                Width = TIMING_COLUMN_WIDTH;
                Spacing = new Vector2(5);
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = group.Time.ToEditorFormattedString(),
                        Font = OsuFont.GetFont(size: TEXT_SIZE, weight: FontWeight.Bold),
                        Width = 70,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    },
                    new ControlGroupAttributes(group, c => c is TimingControlPoint)
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    }
                };
            }
        }

        private partial class ControlGroupAttributes : CompositeDrawable
        {
            private readonly Func<ControlPoint, bool> matchFunction;

            private readonly IBindableList<ControlPoint> controlPoints = new BindableList<ControlPoint>();

            private readonly FillFlowContainer fill;

            public ControlGroupAttributes(ControlPointGroup group, Func<ControlPoint, bool> matchFunction)
            {
                this.matchFunction = matchFunction;

                AutoSizeAxes = Axes.X;
                RelativeSizeAxes = Axes.Y;
                Name = @"ControlGroupAttributes";

                InternalChild = fill = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.X,
                    RelativeSizeAxes = Axes.Y,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(2)
                };

                controlPoints.BindTo(group.ControlPoints);
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                createChildren();
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                controlPoints.CollectionChanged += (_, _) => createChildren();
            }

            private void createChildren()
            {
                fill.ChildrenEnumerable = controlPoints
                                          .Where(matchFunction)
                                          .Select(createAttribute)
                                          // arbitrary ordering to make timing points first.
                                          // probably want to explicitly define order in the future.
                                          .OrderByDescending(c => c.GetType().Name);
            }

            private Drawable createAttribute(ControlPoint controlPoint)
            {
                switch (controlPoint)
                {
                    case TimingControlPoint timing:
                        return new TimingRowAttribute(timing);

                    case DifficultyControlPoint difficulty:
                        return new DifficultyRowAttribute(difficulty);

                    case EffectControlPoint effect:
                        return new EffectRowAttribute(effect);

                    case SampleControlPoint sample:
                        return new SampleRowAttribute(sample);
                }

                throw new ArgumentOutOfRangeException(nameof(controlPoint), $"Control point type {controlPoint.GetType()} is not supported");
            }
        }
    }
}
