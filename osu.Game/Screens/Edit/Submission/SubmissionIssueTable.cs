// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using System.Linq;
using osu.Game.Overlays;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Screens.Edit.Submission
{
    public partial class SubmissionIssueTable : CompositeDrawable
    {
        [Resolved]
        private BindableList<Issue> issues { get; set; } = null!;

        private FillFlowContainer rowFlow = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = Verify.IssueTable.ROW_HEIGHT,
                    Padding = new MarginPadding { Horizontal = Verify.IssueTable.ROW_HORIZONTAL_PADDING },
                    Children = new Drawable[]
                    {
                        new HeaderText("Category")
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                        new HeaderText("Time")
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Margin = new MarginPadding { Left = Verify.IssueTable.COLUMN_WIDTH + Verify.IssueTable.COLUMN_GAP },
                        },
                        new HeaderText("Message")
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Margin = new MarginPadding { Left = 2 * (Verify.IssueTable.COLUMN_WIDTH + Verify.IssueTable.COLUMN_GAP) },
                        },
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = Verify.IssueTable.ROW_HEIGHT },
                    Child = new OsuScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = rowFlow = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            issues.BindCollectionChanged((_, _) => rebuildRows(), true);
        }

        private void rebuildRows()
        {
            rowFlow.Clear(false);

            foreach (var issue in issues
                                  .OrderBy(i => i.Check.Metadata.Category)
                                  .ThenBy(i => i.Time ?? double.MinValue))
            {
                var drawable = new DrawableIssue(issue)
                {
                    RelativeSizeAxes = Axes.X,
                };
                rowFlow.Add(drawable);
            }
        }

        public partial class DrawableIssue : CompositeDrawable
        {
            private readonly Issue issue;

            private Box background = null!;
            private OsuSpriteText issueCategoryText = null!;
            private OsuSpriteText issueTimestampText = null!;
            private OsuTextFlowContainer issueDetailFlow = null!;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            public DrawableIssue(Issue issue)
            {
                this.issue = issue;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                InternalChildren = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding { Horizontal = Verify.IssueTable.ROW_HORIZONTAL_PADDING, Vertical = 4 },
                        Children = new Drawable[]
                        {
                            issueCategoryText = new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = OsuFont.GetFont(size: Verify.IssueTable.TEXT_SIZE, weight: FontWeight.Bold),
                            },
                            issueTimestampText = new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = OsuFont.GetFont(size: Verify.IssueTable.TEXT_SIZE, weight: FontWeight.Bold),
                                Margin = new MarginPadding { Left = Verify.IssueTable.COLUMN_WIDTH + Verify.IssueTable.COLUMN_GAP },
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Padding = new MarginPadding
                                {
                                    Left = 2 * (Verify.IssueTable.COLUMN_GAP + Verify.IssueTable.COLUMN_WIDTH),
                                    Right = 0,
                                },
                                Child = issueDetailFlow = new OsuTextFlowContainer(cp => cp.Font = OsuFont.GetFont(size: Verify.IssueTable.TEXT_SIZE, weight: FontWeight.Medium))
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    TextAnchor = Anchor.TopLeft,
                                }
                            }
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                updateState();
                FinishTransforms(true);
            }

            private void updateState()
            {
                issueCategoryText.Text = issue.Check.Metadata.Category.ToString();
                issueTimestampText.Text = issue.GetEditorTimestamp();
                issueDetailFlow.Text = issue.ToString();

                background.FadeTo(0.15f, 100, Easing.OutQuint);
                background.Colour = colourProvider.Background1;
            }
        }

        private partial class HeaderText : OsuSpriteText
        {
            public HeaderText(string text)
            {
                Text = text;
                Font = OsuFont.GetFont(size: Verify.IssueTable.TEXT_SIZE, weight: FontWeight.Bold);
            }
        }
    }
}
