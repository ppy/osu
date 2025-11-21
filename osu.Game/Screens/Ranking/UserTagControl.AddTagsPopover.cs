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
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Input.Bindings;
using osu.Game.Screens.Ranking.Statistics;
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

            [BackgroundDependencyLoader]
            private void load()
            {
                AllowableAnchors = new[]
                {
                    Anchor.TopCentre,
                    Anchor.BottomCentre,
                };

                Children = new Drawable[]
                {
                    new Container
                    {
                        Size = new Vector2(400, 300),
                        Children = new Drawable[]
                        {
                            searchBox = new SearchTextBox
                            {
                                HoldFocus = true,
                                RelativeSizeAxes = Axes.X,
                                Depth = float.MinValue,
                            },
                            new OsuScrollContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                Y = 40,
                                Height = 260,
                                ScrollbarOverlapsContent = false,
                                Child = searchContainer = new SearchContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding { Right = 5, Bottom = 10 },
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(10),
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
                    Spacing = new Vector2(5);

                    Add(new StatisticItemHeader { Text = name ?? "uncategorised" });

                    FilterTerms = name == null ? [] : [name];
                }
            }

            private partial class DrawableAddableTag : OsuAnimatedButton, IFilterable
            {
                public readonly UserTag Tag;

                private Box votedBackground = null!;
                private SpriteIcon votedIcon = null!;

                private readonly Bindable<bool> voted = new Bindable<bool>();
                private readonly BindableBool updating = new BindableBool();

                private LoadingLayer loadingLayer = null!;

                public DrawableAddableTag(UserTag tag)
                {
                    Tag = tag;

                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;

                    ScaleOnMouseDown = 0.95f;

                    voted.BindTo(Tag.Voted);
                    updating.BindTo(Tag.Updating);
                }

                [Resolved]
                private OsuColour colours { get; set; } = null!;

                [BackgroundDependencyLoader]
                private void load()
                {
                    Content.AddRange(new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.Gray7,
                            Depth = float.MaxValue,
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Y,
                            Width = 30,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Depth = float.MaxValue,
                            Children = new Drawable[]
                            {
                                votedBackground = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                votedIcon = new SpriteIcon
                                {
                                    Size = new Vector2(16),
                                    Icon = FontAwesome.Solid.ThumbsUp,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                }
                            }
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(2),
                            Padding = new MarginPadding(5) { Right = 35 },
                            Children = new Drawable[]
                            {
                                new OsuTextFlowContainer(t => t.Font = OsuFont.Default.With(weight: FontWeight.SemiBold))
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Text = Tag.DisplayName,
                                },
                                new OsuTextFlowContainer(t => t.Font = OsuFont.Default.With(size: 14))
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Text = Tag.Description,
                                }
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
                        votedBackground.FadeColour(voted.Value ? colours.Lime2 : colours.Gray2, 250, Easing.OutQuint);
                        votedIcon.FadeColour(voted.Value ? Colour4.Black : Colour4.White, 250, Easing.OutQuint);
                    }, true);
                    FinishTransforms(true);

                    updating.BindValueChanged(u => loadingLayer.State.Value = u.NewValue ? Visibility.Visible : Visibility.Hidden);
                }
            }
        }
    }
}
