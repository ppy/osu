// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Screens.Edit.Verify
{
    public partial class IssueTable : EditorTable
    {
        private Bindable<Issue> selectedIssue = null!;

        [Resolved]
        private VerifyScreen verify { get; set; } = null!;

        [Resolved]
        private EditorClock clock { get; set; } = null!;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved]
        private Editor editor { get; set; } = null!;

        public IEnumerable<Issue> Issues
        {
            set
            {
                Content = null;
                BackgroundFlow.Clear();

                if (!value.Any())
                    return;

                foreach (var issue in value)
                {
                    BackgroundFlow.Add(new RowBackground(issue)
                    {
                        Action = () =>
                        {
                            selectedIssue.Value = issue;

                            if (issue.Time != null)
                            {
                                clock.Seek(issue.Time.Value);
                                editor.OnPressed(new KeyBindingPressEvent<GlobalAction>(GetContainingInputManager().CurrentState, GlobalAction.EditorComposeMode));
                            }

                            if (!issue.HitObjects.Any())
                                return;

                            editorBeatmap.SelectedHitObjects.Clear();
                            editorBeatmap.SelectedHitObjects.AddRange(issue.HitObjects);
                        },
                    });
                }

                Columns = createHeaders();
                Content = value.Select((g, i) => createContent(i, g)).ToArray().ToRectangular();
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedIssue = verify.SelectedIssue.GetBoundCopy();
            selectedIssue.BindValueChanged(issue =>
            {
                SetSelectedRow(issue.NewValue);
            }, true);
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
    }
}
