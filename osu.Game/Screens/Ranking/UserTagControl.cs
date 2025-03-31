// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.Ranking.Statistics;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Ranking
{
    public partial class UserTagControl : CompositeDrawable
    {
        private readonly BeatmapInfo beatmapInfo;

        public override bool HandlePositionalInput => true;

        private readonly Cached layout = new Cached();

        private FillFlowContainer<DrawableUserTag> tagFlow = null!;
        private LoadingLayer loadingLayer = null!;

        private BindableList<UserTag> displayedTags { get; } = new BindableList<UserTag>();
        private BindableList<UserTag> extraTags { get; } = new BindableList<UserTag>();

        private Bindable<APITag[]?> allTags = null!;
        private readonly Bindable<APIBeatmap?> apiBeatmap = new Bindable<APIBeatmap?>();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        public UserTagControl(BeatmapInfo beatmapInfo)
        {
            this.beatmapInfo = beatmapInfo;
        }

        [BackgroundDependencyLoader]
        private void load(SessionStatics sessionStatics)
        {
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(10),
                    ColumnDimensions =
                    [
                        new Dimension(GridSizeMode.Absolute, 300),
                        new Dimension()
                    ],
                    RowDimensions = [new Dimension(GridSizeMode.AutoSize, minSize: 250)],
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                Direction = FillDirection.Vertical,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Spacing = new Vector2(8),
                                Children = new Drawable[]
                                {
                                    tagFlow = new FillFlowContainer<DrawableUserTag>
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Full,
                                        LayoutDuration = 300,
                                        LayoutEasing = Easing.OutQuint,
                                        Spacing = new Vector2(4),
                                    },
                                },
                            },
                            new TagList
                            {
                                RelativeSizeAxes = Axes.Both,
                                AvailableTags = { BindTarget = extraTags },
                                OnSelected = onExtraTagSelected,
                            }
                        }
                    }
                },
                loadingLayer = new LoadingLayer(dimBackground: true)
                {
                    RelativeSizeAxes = Axes.Both,
                    State = { Value = Visibility.Visible }
                },
            };

            allTags = sessionStatics.GetBindable<APITag[]?>(Static.AllBeatmapTags);

            if (allTags.Value == null)
            {
                var listTagsRequest = new ListTagsRequest();
                listTagsRequest.Success += tags => allTags.Value = tags.Tags.ToArray();
                api.Queue(listTagsRequest);
            }

            var getBeatmapSetRequest = new GetBeatmapSetRequest(beatmapInfo.BeatmapSet!.OnlineID);
            getBeatmapSetRequest.Success += set => apiBeatmap.Value = set.Beatmaps.SingleOrDefault(b => b.MatchesOnlineID(beatmapInfo));
            api.Queue(getBeatmapSetRequest);
        }

        private void onExtraTagSelected(UserTag tag)
        {
            loadingLayer.Show();
            extraTags.Remove(tag);

            var req = new AddBeatmapTagRequest(beatmapInfo.OnlineID, tag.Id);
            req.Success += () =>
            {
                tag.Voted.Value = true;
                tag.VoteCount.Value += 1;
                displayedTags.Add(tag);
                loadingLayer.Hide();
            };
            req.Failure += _ => extraTags.Add(tag);
            api.Queue(req);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            allTags.BindValueChanged(_ => updateTags());
            apiBeatmap.BindValueChanged(_ => updateTags());
            updateTags();

            displayedTags.BindCollectionChanged(displayTags, true);
        }

        private void updateTags()
        {
            if (allTags.Value == null || apiBeatmap.Value?.TopTags == null)
                return;

            var allTagsById = allTags.Value.ToDictionary(t => t.Id);
            var ownTagIds = apiBeatmap.Value.OwnTagIds?.ToHashSet() ?? new HashSet<long>();

            foreach (var topTag in apiBeatmap.Value.TopTags)
            {
                if (allTagsById.Remove(topTag.TagId, out var tag))
                {
                    displayedTags.Add(new UserTag(tag)
                    {
                        VoteCount = { Value = topTag.VoteCount },
                        Voted = { Value = ownTagIds.Contains(tag.Id) }
                    });
                }
            }

            extraTags.AddRange(allTagsById.Select(t => new UserTag(t.Value)));

            loadingLayer.Hide();
        }

        private void displayTags(object? sender, NotifyCollectionChangedEventArgs e)
        {
            var oldItems = tagFlow.ToArray();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    for (int i = 0; i < e.NewItems!.Count; i++)
                    {
                        var tag = (UserTag)e.NewItems[i]!;
                        var drawableTag = new DrawableUserTag(tag);
                        tagFlow.Insert(tagFlow.Count, drawableTag);
                        tag.VoteCount.BindValueChanged(voteCountChanged, true);
                        layout.Invalidate();
                    }

                    break;
                }

                case NotifyCollectionChangedAction.Remove:
                {
                    for (int i = 0; i < e.OldItems!.Count; i++)
                    {
                        var tag = (UserTag)e.OldItems[i]!;
                        tag.VoteCount.ValueChanged -= voteCountChanged;
                        tagFlow.Remove(oldItems[e.OldStartingIndex + i], true);
                    }

                    break;
                }

                case NotifyCollectionChangedAction.Reset:
                {
                    tagFlow.Clear();
                    break;
                }
            }
        }

        private void voteCountChanged(ValueChangedEvent<int> _)
        {
            var tagsWithNoVotes = displayedTags.Where(t => t.VoteCount.Value == 0).ToArray();

            foreach (var tag in tagsWithNoVotes)
            {
                displayedTags.Remove(tag);
                extraTags.Add(tag);
            }

            layout.Invalidate();
        }

        protected override void Update()
        {
            base.Update();

            if (!layout.IsValid && !IsHovered)
            {
                var sortedTags = new Dictionary<UserTag, int>(
                    displayedTags.OrderByDescending(t => t.VoteCount.Value)
                                 .ThenByDescending(t => t.Voted.Value)
                                 .Select((tag, index) => new KeyValuePair<UserTag, int>(tag, index)));

                foreach (var drawableTag in tagFlow)
                    tagFlow.SetLayoutPosition(drawableTag, sortedTags[drawableTag.UserTag]);

                layout.Validate();
            }
        }

        protected override bool OnClick(ClickEvent e) => true;

        private partial class DrawableUserTag : OsuAnimatedButton
        {
            public readonly UserTag UserTag;

            private readonly Bindable<int> voteCount = new Bindable<int>();
            private readonly BindableBool voted = new BindableBool();
            private readonly Bindable<bool> confirmed = new BindableBool();

            private Box mainBackground = null!;
            private Box voteBackground = null!;
            private OsuSpriteText tagCategoryText = null!;
            private OsuSpriteText tagNameText = null!;
            private OsuSpriteText voteCountText = null!;
            private LoadingSpinner spinner = null!;

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            [Resolved]
            private Bindable<WorkingBeatmap> beatmap { get; set; } = null!;

            [Resolved]
            private IAPIProvider api { get; set; } = null!;

            private APIRequest? requestInFlight;

            public DrawableUserTag(UserTag userTag)
            {
                UserTag = userTag;
                voteCount.BindTo(userTag.VoteCount);
                voted.BindTo(userTag.Voted);

                AutoSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;
                CornerRadius = 8;
                Masking = true;
                EdgeEffect = new EdgeEffectParameters
                {
                    Colour = colours.Lime1,
                    Radius = 5,
                    Type = EdgeEffectType.Glow,
                };
                Content.AddRange(new Drawable[]
                {
                    mainBackground = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Depth = float.MaxValue,
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Children = new Drawable[]
                        {
                            tagCategoryText = new OsuSpriteText
                            {
                                Alpha = UserTag.GroupName != null ? 0.6f : 0,
                                Text = UserTag.GroupName ?? default(LocalisableString),
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Margin = new MarginPadding { Horizontal = 6 }
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.Y,
                                AutoSizeAxes = Axes.X,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Alpha = 0.1f,
                                        Blending = BlendingParameters.Additive,
                                    },
                                    tagNameText = new OsuSpriteText
                                    {
                                        Text = UserTag.DisplayName,
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Margin = new MarginPadding { Horizontal = 6 }
                                    },
                                }
                            },
                            new Container
                            {
                                AutoSizeAxes = Axes.Both,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Children = new Drawable[]
                                {
                                    voteBackground = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                    },
                                    voteCountText = new OsuSpriteText
                                    {
                                        Margin = new MarginPadding { Horizontal = 6, Vertical = 3, },
                                    },
                                    spinner = new LoadingSpinner(withBox: true)
                                    {
                                        Alpha = 0,
                                        Size = new Vector2(18),
                                    }
                                }
                            }
                        }
                    }
                });

                TooltipText = UserTag.Description;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                const double transition_duration = 300;

                voteCount.BindValueChanged(_ =>
                {
                    voteCountText.Text = voteCount.Value.ToLocalisableString();
                    confirmed.Value = voteCount.Value >= 10;
                }, true);
                voted.BindValueChanged(v =>
                {
                    if (v.NewValue)
                    {
                        voteBackground.FadeColour(colours.Lime2, transition_duration, Easing.OutQuint);
                        voteCountText.FadeColour(Colour4.Black, transition_duration, Easing.OutQuint);
                    }
                    else
                    {
                        voteBackground.FadeColour(colours.Gray2, transition_duration, Easing.OutQuint);
                        voteCountText.FadeColour(Colour4.White, transition_duration, Easing.OutQuint);
                    }
                }, true);
                confirmed.BindValueChanged(c =>
                {
                    if (c.NewValue)
                    {
                        mainBackground.FadeColour(colours.Lime2, transition_duration, Easing.OutQuint);
                        tagCategoryText.FadeColour(Colour4.Black, transition_duration, Easing.OutQuint);
                        tagNameText.FadeColour(Colour4.Black, transition_duration, Easing.OutQuint);
                        FadeEdgeEffectTo(0.5f, transition_duration, Easing.OutQuint);
                    }
                    else
                    {
                        mainBackground.FadeColour(colours.Gray4, transition_duration, Easing.OutQuint);
                        tagCategoryText.FadeColour(Colour4.White, transition_duration, Easing.OutQuint);
                        tagNameText.FadeColour(Colour4.White, transition_duration, Easing.OutQuint);
                        FadeEdgeEffectTo(0f, transition_duration, Easing.OutQuint);
                    }
                }, true);
                FinishTransforms(true);

                Action = () =>
                {
                    if (requestInFlight != null)
                        return;

                    spinner.Show();

                    APIRequest request;

                    switch (voted.Value)
                    {
                        case true:
                            var removeReq = new RemoveBeatmapTagRequest(beatmap.Value.BeatmapInfo.OnlineID, UserTag.Id);
                            removeReq.Success += () =>
                            {
                                voteCount.Value -= 1;
                                voted.Value = false;
                            };
                            request = removeReq;
                            break;

                        case false:
                            var addReq = new AddBeatmapTagRequest(beatmap.Value.BeatmapInfo.OnlineID, UserTag.Id);
                            addReq.Success += () =>
                            {
                                voteCount.Value += 1;
                                voted.Value = true;
                            };
                            request = addReq;
                            break;
                    }

                    request.Success += () =>
                    {
                        spinner.Hide();
                        requestInFlight = null;
                    };
                    request.Failure += _ =>
                    {
                        spinner.Hide();
                        requestInFlight = null;
                    };
                    api.Queue(requestInFlight = request);
                };
            }
        }

        private partial class TagList : CompositeDrawable
        {
            private SearchTextBox searchBox = null!;
            private SearchContainer searchContainer = null!;

            public BindableList<UserTag> AvailableTags { get; } = new BindableList<UserTag>();

            public Action<UserTag>? OnSelected { get; set; }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Masking = true;
                CornerRadius = 5;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.1f,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            searchBox = new SearchTextBox
                            {
                                HoldFocus = true,
                                RelativeSizeAxes = Axes.X,
                                Depth = float.MinValue,
                                Y = -2, // hacky compensation for masking issues
                            },
                            new OsuScrollContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding(10) { Top = 45, },
                                ScrollbarOverlapsContent = false,
                                Child = searchContainer = new SearchContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(10),
                                }
                            }
                        },
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                AvailableTags.BindCollectionChanged((_, _) =>
                {
                    searchContainer.Clear();
                    searchContainer.ChildrenEnumerable = createItems(AvailableTags);
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

            protected override bool OnKeyDown(KeyDownEvent e)
            {
                if (e.Key == Key.Enter)
                {
                    attemptSelect();
                    return true;
                }

                return base.OnKeyDown(e);
            }

            private void attemptSelect()
            {
                var visibleItems = searchContainer.OfType<DrawableAddableTag>().Where(d => d.IsPresent).ToArray();

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

                public bool FilteringActive { set { } }

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

                public DrawableAddableTag(UserTag tag)
                {
                    Tag = tag;

                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    Content.AddRange(new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.Gray6,
                            Depth = float.MaxValue,
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(2),
                            Padding = new MarginPadding(5),
                            Children = new Drawable[]
                            {
                                new OsuTextFlowContainer(t => t.Font = OsuFont.Default.With(weight: FontWeight.Bold))
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
                        }
                    });
                }

                public IEnumerable<LocalisableString> FilterTerms => [Tag.FullName, Tag.Description];

                public bool MatchingFilter { set => Alpha = value ? 1 : 0; }
                public bool FilteringActive { set { } }

                protected override bool OnMouseDown(MouseDownEvent e)
                {
                    bool result = base.OnMouseDown(e);
                    // slightly dodgy way of overriding the amount of scale-on-click (the default is way too much in this case)
                    ClearTransforms(targetMember: nameof(Scale));
                    Content.ScaleTo(0.95f, 2000, Easing.OutQuint);
                    return result;
                }
            }
        }
    }
}
