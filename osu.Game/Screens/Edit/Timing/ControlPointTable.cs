// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Timing
{
    public class ControlPointTable : TableContainer
    {
        private const float horizontal_inset = 20;
        private const float row_height = 25;
        private const int text_size = 14;

        private readonly FillFlowContainer backgroundFlow;

        [Resolved]
        private Bindable<ControlPointGroup> selectedGroup { get; set; }

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

        public IEnumerable<ControlPointGroup> ControlGroups
        {
            set
            {
                Content = null;
                backgroundFlow.Clear();

                if (value?.Any() != true)
                    return;

                foreach (var group in value)
                {
                    backgroundFlow.Add(new RowBackground(group));
                }

                Columns = createHeaders();
                Content = value.Select((g, i) => createContent(i, g)).ToArray().ToRectangular();
            }
        }

        private TableColumn[] createHeaders()
        {
            var columns = new List<TableColumn>
            {
                new TableColumn(string.Empty, Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
                new TableColumn("Time", Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
                new TableColumn("Attributes", Anchor.Centre),
            };

            return columns.ToArray();
        }

        private Drawable[] createContent(int index, ControlPointGroup group) => new Drawable[]
        {
            new OsuSpriteText
            {
                Text = $"#{index + 1}",
                Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Bold),
                Margin = new MarginPadding(10)
            },
            new OsuSpriteText
            {
                Text = group.Time.ToEditorFormattedString(),
                Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Bold)
            },
            new ControlGroupAttributes(group),
        };

        private class ControlGroupAttributes : CompositeDrawable
        {
            private readonly IBindableList<ControlPoint> controlPoints = new BindableList<ControlPoint>();

            private readonly FillFlowContainer fill;

            public ControlGroupAttributes(ControlPointGroup group)
            {
                InternalChild = fill = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Padding = new MarginPadding(10),
                    Spacing = new Vector2(2)
                };

                controlPoints.BindTo(group.ControlPoints);
            }

            [Resolved]
            private OsuColour colours { get; set; }

            [BackgroundDependencyLoader]
            private void load()
            {
                createChildren();
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                controlPoints.CollectionChanged += (_, __) => createChildren();
            }

            private void createChildren()
            {
                fill.ChildrenEnumerable = controlPoints.Select(createAttribute).Where(c => c != null);
            }

            private Drawable createAttribute(ControlPoint controlPoint)
            {
                Color4 colour = controlPoint.GetRepresentingColour(colours);

                switch (controlPoint)
                {
                    case TimingControlPoint timing:
                        return new RowAttribute("timing", () => $"{60000 / timing.BeatLength:n1}bpm {timing.TimeSignature}", colour);

                    case DifficultyControlPoint difficulty:

                        return new RowAttribute("difficulty", () => $"{difficulty.SpeedMultiplier:n2}x", colour);

                    case EffectControlPoint effect:
                        return new RowAttribute("effect", () => $"{(effect.KiaiMode ? "Kiai " : "")}{(effect.OmitFirstBarLine ? "NoBarLine " : "")}", colour);

                    case SampleControlPoint sample:
                        return new RowAttribute("sample", () => $"{sample.SampleBank} {sample.SampleVolume}%", colour);
                }

                return null;
            }
        }

        protected override Drawable CreateHeader(int index, TableColumn column) => new HeaderText(column?.Header ?? string.Empty);

        private class HeaderText : OsuSpriteText
        {
            public HeaderText(string text)
            {
                Text = text.ToUpper();
                Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold);
            }
        }

        public class RowBackground : OsuClickableContainer
        {
            private readonly ControlPointGroup controlGroup;
            private const int fade_duration = 100;

            private readonly Box hoveredBackground;

            [Resolved]
            private EditorClock clock { get; set; }

            [Resolved]
            private Bindable<ControlPointGroup> selectedGroup { get; set; }

            public RowBackground(ControlPointGroup controlGroup)
            {
                this.controlGroup = controlGroup;
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

                Action = () =>
                {
                    selectedGroup.Value = controlGroup;
                    clock.SeekTo(controlGroup.Time);
                };
            }

            private Color4 colourHover;
            private Color4 colourSelected;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                hoveredBackground.Colour = colourHover = colours.BlueDarker;
                colourSelected = colours.YellowDarker;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                selectedGroup.BindValueChanged(group => { Selected = controlGroup == group.NewValue; }, true);
            }

            private bool selected;

            protected bool Selected
            {
                get => selected;
                set
                {
                    if (value == selected)
                        return;

                    selected = value;
                    updateState();
                }
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateState();
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                updateState();
                base.OnHoverLost(e);
            }

            private void updateState()
            {
                hoveredBackground.FadeColour(selected ? colourSelected : colourHover, 450, Easing.OutQuint);

                if (selected || IsHovered)
                    hoveredBackground.FadeIn(fade_duration, Easing.OutQuint);
                else
                    hoveredBackground.FadeOut(fade_duration, Easing.OutQuint);
            }
        }
    }
}
