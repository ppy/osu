// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Submission
{
    public partial class SubmissionIssueTable : CompositeDrawable
    {
        public BindableList<Issue> Issues { get; } = new BindableList<Issue>();

        public const float COLUMN_WIDTH = 70;
        public const float COLUMN_GAP = 10;
        public const float ROW_HEIGHT = 25;
        public const float ROW_HORIZONTAL_PADDING = 20;
        public const int TEXT_SIZE = 14;

        public void SetIssues(IEnumerable<Issue> issues)
        {
            Issues.Clear();
            Issues.AddRange(issues);
        }

        private FillFlowContainer rowFlow = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = ROW_HEIGHT,
                    Padding = new MarginPadding { Horizontal = ROW_HORIZONTAL_PADDING },
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
                            Margin = new MarginPadding { Left = COLUMN_WIDTH + COLUMN_GAP },
                        },
                        new HeaderText("Message")
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Margin = new MarginPadding { Left = 2 * (COLUMN_WIDTH + COLUMN_GAP) },
                        },
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = ROW_HEIGHT },
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

            Issues.BindCollectionChanged((_, __) => rebuildRows(), true);
        }

        private void rebuildRows()
        {
            rowFlow.Clear(false);

            foreach (var issue in Issues)
            {
                var drawable = new DrawableIssue
                {
                    RelativeSizeAxes = Axes.X,
                };
                drawable.Current.Value = issue;
                rowFlow.Add(drawable);
            }
        }

        public partial class DrawableIssue : CompositeDrawable, IHasCurrentValue<Issue>
        {
            private readonly BindableWithCurrent<Issue> current = new BindableWithCurrent<Issue>();

            private Box background = null!;
            private OsuSpriteText issueCategoryText = null!;
            private OsuSpriteText issueTimestampText = null!;
            private OsuTextFlowContainer issueDetailFlow = null!;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            public Bindable<Issue> Current
            {
                get => current.Current;
                set => current.Current = value;
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
                        Padding = new MarginPadding { Horizontal = ROW_HORIZONTAL_PADDING, Vertical = 4 },
                        Children = new Drawable[]
                        {
                            issueCategoryText = new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = OsuFont.GetFont(size: TEXT_SIZE, weight: FontWeight.Bold),
                            },
                            issueTimestampText = new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = OsuFont.GetFont(size: TEXT_SIZE, weight: FontWeight.Bold),
                                Margin = new MarginPadding { Left = COLUMN_WIDTH + COLUMN_GAP },
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Padding = new MarginPadding
                                {
                                    Left = 2 * (COLUMN_GAP + COLUMN_WIDTH),
                                    Right = 0,
                                },
                                Child = issueDetailFlow = new OsuTextFlowContainer(cp => cp.Font = OsuFont.GetFont(size: TEXT_SIZE, weight: FontWeight.Medium))
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                }
                            }
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Current.BindValueChanged(_ => updateState(), true);
                FinishTransforms(true);
            }

            private void updateState()
            {
                issueCategoryText.Text = Current.Value.Check.Metadata.Category.ToString();
                issueTimestampText.Text = Current.Value.GetEditorTimestamp();
                issueDetailFlow.Text = Current.Value.ToString();

                background.FadeTo(0.15f, 100, Easing.OutQuint);
                background.Colour = colourProvider.Background1;
            }
        }

        private partial class HeaderText : OsuSpriteText
        {
            public HeaderText(string text)
            {
                Text = text;
                Font = OsuFont.GetFont(size: TEXT_SIZE, weight: FontWeight.Bold);
            }
        }
    }
}


