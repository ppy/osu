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
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Edit.Checks.Components;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Verify
{
    public class IssueTable : EditorTable
    {
        [Resolved]
        private Bindable<Issue> selectedIssue { get; set; }

        public IEnumerable<Issue> Issues
        {
            set
            {
                Content = null;
                BackgroundFlow.Clear();

                if (value == null)
                    return;

                foreach (var issue in value)
                {
                    BackgroundFlow.Add(new RowBackground(issue));
                }

                Columns = createHeaders();
                Content = value.Select((g, i) => createContent(i, g)).ToArray().ToRectangular();
            }
        }

        private TableColumn[] createHeaders()
        {
            var columns = new List<TableColumn>
            {
                new TableColumn(string.Empty, Anchor.CentreLeft, new Dimension(GridSizeMode.AutoSize)),
                new TableColumn("Type", Anchor.CentreLeft, new Dimension(GridSizeMode.AutoSize, minSize: 60)),
                new TableColumn("Time", Anchor.CentreLeft, new Dimension(GridSizeMode.AutoSize, minSize: 60)),
                new TableColumn("Message", Anchor.CentreLeft),
                new TableColumn("Category", Anchor.CentreRight, new Dimension(GridSizeMode.AutoSize)),
            };

            return columns.ToArray();
        }

        private Drawable[] createContent(int index, Issue issue) => new Drawable[]
        {
            new OsuSpriteText
            {
                Text = $"#{index + 1}",
                Font = OsuFont.GetFont(size: TEXT_SIZE, weight: FontWeight.Medium),
                Margin = new MarginPadding { Right = 10 }
            },
            new OsuSpriteText
            {
                Text = issue.Template.Type.ToString(),
                Font = OsuFont.GetFont(size: TEXT_SIZE, weight: FontWeight.Bold),
                Margin = new MarginPadding { Right = 10 },
                Colour = issue.Template.Colour
            },
            new OsuSpriteText
            {
                Text = issue.GetEditorTimestamp(),
                Font = OsuFont.GetFont(size: TEXT_SIZE, weight: FontWeight.Bold),
                Margin = new MarginPadding { Right = 10 },
            },
            new OsuSpriteText
            {
                Text = issue.ToString(),
                Font = OsuFont.GetFont(size: TEXT_SIZE, weight: FontWeight.Medium)
            },
            new OsuSpriteText
            {
                Text = issue.Check.Metadata.Category.ToString(),
                Font = OsuFont.GetFont(size: TEXT_SIZE, weight: FontWeight.Bold),
                Margin = new MarginPadding(10)
            }
        };

        public class RowBackground : OsuClickableContainer
        {
            private readonly Issue issue;
            private const int fade_duration = 100;

            private readonly Box hoveredBackground;

            [Resolved]
            private EditorClock clock { get; set; }

            [Resolved]
            private Editor editor { get; set; }

            [Resolved]
            private EditorBeatmap editorBeatmap { get; set; }

            [Resolved]
            private Bindable<Issue> selectedIssue { get; set; }

            public RowBackground(Issue issue)
            {
                this.issue = issue;

                RelativeSizeAxes = Axes.X;
                Height = ROW_HEIGHT;
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

                    if (issue.Time != null)
                    {
                        clock.Seek(issue.Time.Value);
                        editor.OnPressed(GlobalAction.EditorComposeMode);
                    }

                    if (!issue.HitObjects.Any())
                        return;

                    editorBeatmap.SelectedHitObjects.Clear();
                    editorBeatmap.SelectedHitObjects.AddRange(issue.HitObjects);
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

                selectedIssue.BindValueChanged(change => { Selected = issue == change.NewValue; }, true);
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
