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
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Verify
{
    public class IssueTable : TableContainer
    {
        private const float horizontal_inset = 20;
        private const float row_height = 25;
        private const int text_size = 14;

        private readonly FillFlowContainer backgroundFlow;

        public IssueTable()
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

        public IEnumerable<Issue> Issues
        {
            set
            {
                Content = null;
                backgroundFlow.Clear();

                if (value?.Any() != true)
                    return;

                foreach (var issue in value)
                {
                    backgroundFlow.Add(new IssueTable.RowBackground(issue));
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
                new TableColumn(),
                new TableColumn("Attributes", Anchor.CentreLeft),
            };

            return columns.ToArray();
        }

        private Drawable[] createContent(int index, Issue issue) => new Drawable[]
        {
            new OsuSpriteText
            {
                Text = $"#{index + 1}",
                Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Bold),
                Margin = new MarginPadding(10)
            },
            new OsuSpriteText
            {
                Text = issue.Time.ToEditorFormattedString(),
                Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Bold)
            },
            null,
            null //new ControlGroupAttributes(issue),
        };

        public class RowBackground : OsuClickableContainer
        {
            private readonly Issue issue;
            private const int fade_duration = 100;

            private readonly Box hoveredBackground;

            [Resolved]
            private EditorClock clock { get; set; }

            [Resolved]
            private Bindable<Issue> selectedIssue { get; set; }

            public RowBackground(Issue issue)
            {
                this.issue = issue;
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
                    selectedIssue.Value = issue;
                    clock.SeekSmoothlyTo(issue.Time);
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

                selectedIssue.BindValueChanged(group => { Selected = issue == group.NewValue; }, true);
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
