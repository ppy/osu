// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Ranking
{
    public partial class UserTagControl
    {
        private partial class AddTagsPopover : OsuPopover
        {
            private SearchTextBox searchBox = null!;
            private SearchContainer searchContainer = null!;

            public BindableDictionary<long, UserTag> AvailableTags { get; } = new BindableDictionary<long, UserTag>();

            public Action<UserTag>? OnSelected { get; set; }

            private CancellationTokenSource? loadCancellationTokenSource;

            public AddTagsPopover()
                : base(withPadding: false)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                AllowableAnchors = new[]
                {
                    Anchor.TopCentre,
                    Anchor.BottomCentre,
                };

                Content.Padding = new MarginPadding { Vertical = 20 };

                Children = new Drawable[]
                {
                    new Container
                    {
                        Size = new Vector2(400, 300),
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                Padding = new MarginPadding { Horizontal = 20 },
                                Child = searchBox = new SearchTextBox
                                {
                                    HoldFocus = true,
                                    RelativeSizeAxes = Axes.X,
                                    Depth = float.MinValue,
                                },
                            },
                            new OsuScrollContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                Y = 40,
                                Height = 260,
                                Child = searchContainer = new SearchContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding { Horizontal = 20 },
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(2.5f),
                                }
                            }
                        },
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                AvailableTags.BindCollectionChanged((_, _) =>
                {
                    loadCancellationTokenSource?.Cancel();
                    loadCancellationTokenSource = new CancellationTokenSource();

                    LoadComponentsAsync(createItems(AvailableTags.Values), loaded =>
                    {
                        searchContainer.Clear();
                        searchContainer.AddRange(loaded);
                    }, loadCancellationTokenSource.Token);
                }, true);
                searchBox.Current.BindValueChanged(_ => searchContainer.SearchTerm = searchBox.Current.Value, true);
            }

            private IEnumerable<Drawable> createItems(IEnumerable<UserTag> tags)
            {
                var grouped = tags.GroupBy(tag => tag.GroupName).OrderBy(group => group.Key);

                foreach (var group in grouped)
                {
                    var drawableGroup = new GroupFlow(group.Key);

                    foreach (var tag in group.OrderBy(t => t.FullName))
                        drawableGroup.Add(new DrawableAddableTag(tag) { Action = () => OnSelected?.Invoke(tag) });

                    yield return drawableGroup;
                }
            }

            public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
            {
                if (e.Action == GlobalAction.Select && !e.Repeat)
                {
                    attemptSelect();
                    return true;
                }

                return false;
            }

            private void attemptSelect()
            {
                var visibleItems = searchContainer.ChildrenOfType<DrawableAddableTag>().Where(d => d.IsPresent).ToArray();

                if (visibleItems.Length == 1)
                    OnSelected?.Invoke(visibleItems.Single().Tag);
            }

            private partial class GroupFlow : FillFlowContainer, IFilterable
            {
                public IEnumerable<LocalisableString> FilterTerms { get; }

                private readonly Circle circle;

                public bool MatchingFilter
                {
                    set => Alpha = value ? 1 : 0;
                }

                public bool FilteringActive
                {
                    set { }
                }

                public GroupFlow(string? name)
                {
                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;
                    Direction = FillDirection.Vertical;
                    Spacing = new Vector2(2.5f);

                    Add(new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(5, 0),
                        Margin = new MarginPadding { Vertical = 2.5f },
                        Children = new Drawable[]
                        {
                            circle = new Circle
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Height = 12,
                                Width = 6,
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Text = name ?? "uncategorized",
                                Font = OsuFont.Style.Heading2,
                            }
                        },
                    });

                    FilterTerms = name == null ? [] : [name];
                }

                [BackgroundDependencyLoader]
                private void load(OverlayColourProvider colourProvider)
                {
                    circle.Colour = colourProvider.Highlight1;
                }
            }

            private partial class DrawableAddableTag : OsuAnimatedButton, IFilterable
            {
                public readonly UserTag Tag;

                private Box background = null!;

                private readonly Bindable<bool> voted = new Bindable<bool>();
                private readonly BindableBool updating = new BindableBool();

                private LoadingLayer loadingLayer = null!;

                public DrawableAddableTag(UserTag tag)
                {
                    Tag = tag;

                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;

                    Content.CornerRadius = 4;

                    ScaleOnMouseDown = 0.95f;

                    voted.BindTo(Tag.Voted);
                    updating.BindTo(Tag.Updating);
                }

                [Resolved]
                private OverlayColourProvider colourProvider { get; set; } = null!;

                [BackgroundDependencyLoader]
                private void load()
                {
                    Content.AddRange(new Drawable[]
                    {
                        background = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background3,
                            Depth = float.MaxValue,
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Horizontal = 10, Vertical = 5 },
                            ColumnDimensions = new[]
                            {
                                new Dimension(GridSizeMode.Absolute, 100),
                                new Dimension(GridSizeMode.Absolute, 10),
                                new Dimension(),
                            },
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                            },
                            Content = new[]
                            {
                                new Drawable?[]
                                {
                                    new OsuTextFlowContainer(t => t.Font = OsuFont.Style.Caption1.With(weight: FontWeight.Bold))
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Text = Tag.DisplayName,
                                    },
                                    null,
                                    new OsuTextFlowContainer(t => t.Font = OsuFont.Style.Caption2)
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Text = Tag.Description,
                                    }
                                },
                            }
                        },
                        loadingLayer = new LoadingLayer(dimBackground: true),
                    });
                }

                public IEnumerable<LocalisableString> FilterTerms => [Tag.FullName, Tag.Description];

                public bool MatchingFilter
                {
                    set => Alpha = value ? 1 : 0;
                }

                public bool FilteringActive
                {
                    set { }
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();

                    voted.BindValueChanged(_ =>
                    {
                        background.FadeColour(voted.Value ? colourProvider.Highlight2 : colourProvider.Background3, 250, Easing.OutQuint);
                    }, true);
                    FinishTransforms(true);

                    updating.BindValueChanged(u => loadingLayer.State.Value = u.NewValue ? Visibility.Visible : Visibility.Hidden);
                }
            }
        }
    }
}
