// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Screens.Edit.Verify
{
    public partial class IssueTable : CompositeDrawable
    {
        public BindableList<Issue> Issues { get; } = new BindableList<Issue>();

        public const float COLUMN_WIDTH = 70;
        public const float COLUMN_GAP = 10;
        public const float ROW_HEIGHT = 25;
        public const float ROW_HORIZONTAL_PADDING = 20;
        public const int TEXT_SIZE = 14;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = ROW_HEIGHT,
                    Padding = new MarginPadding { Horizontal = ROW_HORIZONTAL_PADDING, },
                    Children = new[]
                    {
                        new TableHeaderText("Type")
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                        new TableHeaderText("Time")
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Margin = new MarginPadding { Left = COLUMN_WIDTH + COLUMN_GAP },
                        },
                        new TableHeaderText("Message")
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Margin = new MarginPadding { Left = 2 * (COLUMN_WIDTH + COLUMN_GAP) },
                        },
                        new TableHeaderText("Category")
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                        },
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = ROW_HEIGHT, },
                    Child = new IssueRowList
                    {
                        RelativeSizeAxes = Axes.Both,
                        RowData = { BindTarget = Issues }
                    }
                }
            };
        }

        private partial class IssueRowList : VirtualisedListContainer<Issue, DrawableIssue>
        {
            public IssueRowList()
                : base(ROW_HEIGHT, 50)
            {
            }

            protected override ScrollContainer<Drawable> CreateScrollContainer() => new OsuScrollContainer();
        }

        public partial class DrawableIssue : PoolableDrawable, IHasCurrentValue<Issue>
        {
            private readonly BindableWithCurrent<Issue> current = new BindableWithCurrent<Issue>();

            private readonly Bindable<Issue> selectedIssue = new Bindable<Issue>();

            private Box background = null!;
            private OsuSpriteText issueTypeText = null!;
            private OsuSpriteText issueTimestampText = null!;
            private OsuSpriteText issueDetailText = null!;
            private OsuSpriteText issueCategoryText = null!;

            [Resolved]
            private EditorClock clock { get; set; } = null!;

            [Resolved]
            private EditorBeatmap editorBeatmap { get; set; } = null!;

            [Resolved]
            private Editor editor { get; set; } = null!;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            public Bindable<Issue> Current
            {
                get => current.Current;
                set => current.Current = value;
            }

            [BackgroundDependencyLoader]
            private void load(VerifyScreen verify)
            {
                RelativeSizeAxes = Axes.X;
                Height = ROW_HEIGHT;

                InternalChildren = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Horizontal = 20, },
                        Children = new Drawable[]
                        {
                            issueTypeText = new OsuSpriteText
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
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding
                                {
                                    Left = 2 * (COLUMN_GAP + COLUMN_WIDTH),
                                    Right = COLUMN_GAP + COLUMN_WIDTH,
                                },
                                Child = issueDetailText = new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Font = OsuFont.GetFont(size: TEXT_SIZE, weight: FontWeight.Medium)
                                },
                            },
                            issueCategoryText = new OsuSpriteText
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Font = OsuFont.GetFont(size: TEXT_SIZE, weight: FontWeight.Bold),
                            }
                        }
                    }
                };

                selectedIssue.BindTo(verify.SelectedIssue);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                selectedIssue.BindValueChanged(_ => updateState());
                Current.BindValueChanged(_ => updateState(), true);
                FinishTransforms(true);
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateState();
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                updateState();
                base.OnHoverLost(e);
            }

            protected override bool OnClick(ClickEvent e)
            {
                selectedIssue.Value = current.Value;

                if (current.Value.Time != null)
                {
                    clock.Seek(current.Value.Time.Value);
                    editor.OnPressed(new KeyBindingPressEvent<GlobalAction>(GetContainingInputManager()!.CurrentState, GlobalAction.EditorComposeMode));
                }

                if (current.Value.HitObjects.Any())
                {
                    editorBeatmap.SelectedHitObjects.Clear();
                    editorBeatmap.SelectedHitObjects.AddRange(current.Value.HitObjects);
                }

                return true;
            }

            private void updateState()
            {
                issueTypeText.Text = Current.Value.Template.Type.ToString();
                issueTypeText.Colour = Current.Value.Template.Colour;
                issueTimestampText.Text = Current.Value.GetEditorTimestamp();
                issueDetailText.Text = Current.Value.ToString();
                issueCategoryText.Text = Current.Value.Check.Metadata.Category.ToString();

                bool isSelected = selectedIssue.Value == current.Value;

                if (IsHovered || isSelected)
                    background.FadeIn(100, Easing.OutQuint);
                else
                    background.FadeOut(100, Easing.OutQuint);

                background.Colour = isSelected ? colourProvider.Colour3 : colourProvider.Background1;
            }
        }
    }
}
