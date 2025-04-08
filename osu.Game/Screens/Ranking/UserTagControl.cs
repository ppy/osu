// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Input.Bindings;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Screens.Ranking.Statistics;
using osuTK;

namespace osu.Game.Screens.Ranking
{
    public partial class UserTagControl : CompositeDrawable
    {
        private readonly BeatmapInfo beatmapInfo;

        public override bool HandlePositionalInput => true;

        private readonly Cached layout = new Cached();

        private FillFlowContainer<DrawableUserTag> tagFlow = null!;

        private BindableList<UserTag> displayedTags { get; } = new BindableList<UserTag>();

        private Bindable<APITag[]?> apiTags = null!;
        private BindableDictionary<long, UserTag> relevantTagsById { get; } = new BindableDictionary<long, UserTag>();

        private readonly Bindable<APIBeatmap?> apiBeatmap = new Bindable<APIBeatmap?>();

        private AddNewTagUserTag addNewTagUserTag = null!;

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
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize)
                    ],
                    RowDimensions = [new Dimension(GridSizeMode.AutoSize, minSize: 40)],
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
                                        Spacing = new Vector2(4),
                                        Child = addNewTagUserTag = new AddNewTagUserTag
                                        {
                                            AvailableTags = { BindTarget = relevantTagsById },
                                            OnTagSelected = toggleVote,
                                        },
                                    },
                                },
                            },
                        }
                    }
                },
            };

            apiTags = sessionStatics.GetBindable<APITag[]?>(Static.AllBeatmapTags);

            if (apiTags.Value == null)
            {
                var listTagsRequest = new ListTagsRequest();
                listTagsRequest.Success += tags => apiTags.Value = tags.Tags.ToArray();
                api.Queue(listTagsRequest);
            }

            var getBeatmapSetRequest = new GetBeatmapSetRequest(beatmapInfo.BeatmapSet!.OnlineID);
            getBeatmapSetRequest.Success += set => apiBeatmap.Value = set.Beatmaps.SingleOrDefault(b => b.MatchesOnlineID(beatmapInfo));
            api.Queue(getBeatmapSetRequest);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            apiTags.BindValueChanged(_ => updateTags());
            apiBeatmap.BindValueChanged(_ => updateTags());
            updateTags();

            displayedTags.BindCollectionChanged(displayTags, true);
        }

        private void updateTags()
        {
            if (apiTags.Value == null || apiBeatmap.Value == null)
                return;

            relevantTagsById.Clear();
            relevantTagsById.AddRange(apiTags.Value
                                             .Where(t => t.RulesetId == null || t.RulesetId == beatmapInfo.Ruleset.OnlineID)
                                             .Select(t => new KeyValuePair<long, UserTag>(t.Id, new UserTag(t))));

            foreach (var topTag in apiBeatmap.Value.TopTags ?? [])
            {
                if (relevantTagsById.TryGetValue(topTag.TagId, out var tag))
                {
                    tag.VoteCount.Value = topTag.VoteCount;
                    tag.Updating.Value = false;
                    displayedTags.Add(tag);
                }
            }

            foreach (long ownTagId in apiBeatmap.Value.OwnTagIds ?? [])
            {
                if (relevantTagsById.TryGetValue(ownTagId, out var tag))
                {
                    tag.Voted.Value = true;
                    tag.Updating.Value = false;
                }
            }
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
                        var drawableTag = new DrawableUserTag(tag) { OnSelected = toggleVote };
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
                        tagFlow.Remove(oldItems[1 + e.OldStartingIndex + i], true);
                    }

                    break;
                }

                case NotifyCollectionChangedAction.Reset:
                {
                    tagFlow.Clear();
                    tagFlow.Add(addNewTagUserTag);
                    break;
                }
            }
        }

        private void toggleVote(UserTag tag)
        {
            if (tag.Updating.Value)
                return;

            tag.Updating.Value = true;

            APIRequest request;

            switch (tag.Voted.Value)
            {
                case true:
                    var removeReq = new RemoveBeatmapTagRequest(beatmapInfo.OnlineID, tag.Id);
                    removeReq.Success += () =>
                    {
                        tag.VoteCount.Value -= 1;
                        tag.Voted.Value = false;
                    };
                    request = removeReq;
                    break;

                case false:
                    var addReq = new AddBeatmapTagRequest(beatmapInfo.OnlineID, tag.Id);
                    addReq.Success += () =>
                    {
                        tag.VoteCount.Value += 1;
                        tag.Voted.Value = true;
                        if (!displayedTags.Contains(tag))
                            displayedTags.Add(tag);
                    };
                    request = addReq;
                    break;
            }

            request.Success += () => tag.Updating.Value = false;
            request.Failure += _ => tag.Updating.Value = false;

            api.Queue(request);
        }

        private void voteCountChanged(ValueChangedEvent<int> _)
        {
            var tagsWithNoVotes = displayedTags.Where(t => t.VoteCount.Value == 0).ToArray();

            foreach (var tag in tagsWithNoVotes)
                displayedTags.Remove(tag);

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
                {
                    if (drawableTag == addNewTagUserTag)
                        tagFlow.SetLayoutPosition(drawableTag, float.MinValue);
                    else
                        tagFlow.SetLayoutPosition(drawableTag, sortedTags[drawableTag.UserTag]);
                }

                layout.Validate();
            }
        }

        protected override bool OnClick(ClickEvent e) => true;

        private partial class DrawableUserTag : OsuAnimatedButton
        {
            public readonly UserTag UserTag;

            public Action<UserTag>? OnSelected { get; set; }

            private readonly Bindable<int> voteCount = new Bindable<int>();
            private readonly BindableBool voted = new BindableBool();
            private readonly Bindable<bool> confirmed = new BindableBool();
            private readonly BindableBool updating = new BindableBool();

            protected Box MainBackground { get; private set; } = null!;
            private Box voteBackground = null!;

            protected OsuSpriteText TagCategoryText { get; private set; } = null!;
            protected OsuSpriteText TagNameText { get; private set; } = null!;
            protected VoteCountText VoteCountText { get; private set; } = null!;

            private readonly bool showVoteCount;

            private LoadingLayer loadingLayer = null!;

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            public DrawableUserTag(UserTag userTag, bool showVoteCount = true)
            {
                UserTag = userTag;
                this.showVoteCount = showVoteCount;
                voteCount.BindTo(userTag.VoteCount);
                updating.BindTo(userTag.Updating);
                voted.BindTo(userTag.Voted);

                AutoSizeAxes = Axes.Both;

                ScaleOnMouseDown = 0.95f;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                CornerRadius = 5;
                Masking = true;
                EdgeEffect = new EdgeEffectParameters
                {
                    Colour = colours.Lime1,
                    Radius = 6,
                    Type = EdgeEffectType.Glow,
                };
                Content.AddRange(new Drawable[]
                {
                    MainBackground = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Depth = float.MaxValue,
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Children = new[]
                        {
                            TagCategoryText = new OsuSpriteText
                            {
                                Alpha = UserTag.GroupName != null ? 0.6f : 0,
                                Text = UserTag.GroupName ?? default(LocalisableString),
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Margin = new MarginPadding { Horizontal = 6 }
                            },
                            new Container
                            {
                                AutoSizeAxes = Axes.Both,
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
                                    TagNameText = new OsuSpriteText
                                    {
                                        Text = UserTag.DisplayName,
                                        Font = OsuFont.Default.With(weight: FontWeight.SemiBold),
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Margin = new MarginPadding { Horizontal = 6, Vertical = 3, },
                                    },
                                }
                            },
                            showVoteCount
                                ? new Container
                                {
                                    RelativeSizeAxes = Axes.Y,
                                    AutoSizeAxes = Axes.X,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Children = new Drawable[]
                                    {
                                        voteBackground = new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                        VoteCountText = new VoteCountText(voteCount)
                                        {
                                            Margin = new MarginPadding { Horizontal = 6 },
                                        },
                                    }
                                }
                                : Empty(),
                        }
                    },
                    loadingLayer = new LoadingLayer(dimBackground: true),
                });

                TooltipText = UserTag.Description;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                const double transition_duration = 300;

                updating.BindValueChanged(u => loadingLayer.State.Value = u.NewValue ? Visibility.Visible : Visibility.Hidden);

                if (showVoteCount)
                {
                    voteCount.BindValueChanged(_ =>
                    {
                        confirmed.Value = voteCount.Value >= 10;
                    }, true);
                    voted.BindValueChanged(v =>
                    {
                        if (v.NewValue)
                        {
                            voteBackground.FadeColour(colours.Lime2, transition_duration, Easing.OutQuint);
                            VoteCountText.FadeColour(Colour4.Black, transition_duration, Easing.OutQuint);
                        }
                        else
                        {
                            voteBackground.FadeColour(colours.Gray2, transition_duration, Easing.OutQuint);
                            VoteCountText.FadeColour(Colour4.White, transition_duration, Easing.OutQuint);
                        }
                    }, true);

                    confirmed.BindValueChanged(c =>
                    {
                        if (c.NewValue)
                        {
                            MainBackground.FadeColour(colours.Lime2, transition_duration, Easing.OutQuint);
                            TagCategoryText.FadeColour(Colour4.Black, transition_duration, Easing.OutQuint);
                            TagNameText.FadeColour(Colour4.Black, transition_duration, Easing.OutQuint);
                            FadeEdgeEffectTo(0.3f, transition_duration, Easing.OutQuint);
                        }
                        else
                        {
                            MainBackground.FadeColour(colours.Gray6, transition_duration, Easing.OutQuint);
                            TagCategoryText.FadeColour(Colour4.White, transition_duration, Easing.OutQuint);
                            TagNameText.FadeColour(Colour4.White, transition_duration, Easing.OutQuint);
                            FadeEdgeEffectTo(0f, transition_duration, Easing.OutQuint);
                        }
                    }, true);
                }

                FinishTransforms(true);

                Action = () => OnSelected?.Invoke(UserTag);
            }
        }

        private partial class AddNewTagUserTag : DrawableUserTag, IHasPopover
        {
            public BindableDictionary<long, UserTag> AvailableTags { get; } = new BindableDictionary<long, UserTag>();

            public Action<UserTag>? OnTagSelected { get; set; }

            [Resolved]
            private OverlayColourProvider overlayColourProvider { get; set; } = null!;

            public AddNewTagUserTag()
                : base(new UserTag(new APITag { Name = "+/add" }), false)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                AvailableTags.BindCollectionChanged((_, _) => Enabled.Value = AvailableTags.Count > 0, true);
                Action = this.ShowPopover;

                MainBackground.FadeColour(overlayColourProvider.Background2);
                TagCategoryText.FadeColour(overlayColourProvider.Colour0);
                TagNameText.FadeColour(overlayColourProvider.Colour0);
                FadeEdgeEffectTo(0);
            }

            public Popover GetPopover() => new AddTagsPopover
            {
                AvailableTags = { BindTarget = AvailableTags },
                OnSelected = OnTagSelected,
            };
        }

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

                public bool MatchingFilter { set => Alpha = value ? 1 : 0; }
                public bool FilteringActive { set { } }

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

        private partial class VoteCountText : CompositeDrawable
        {
            private OsuSpriteText? text;

            private readonly Bindable<int> voteCount;

            public VoteCountText(Bindable<int> voteCount)
            {
                RelativeSizeAxes = Axes.Y;
                AutoSizeAxes = Axes.X;

                this.voteCount = voteCount.GetBoundCopy();
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                voteCount.BindValueChanged(count =>
                {
                    OsuSpriteText? previousText = text;

                    AddInternal(text = new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                        Text = voteCount.Value.ToLocalisableString(),
                    });

                    if (previousText != null)
                    {
                        const double transition_duration = 500;

                        bool isIncrease = count.NewValue > count.OldValue;

                        text.MoveToY(isIncrease ? 20 : -20)
                            .MoveToY(0, transition_duration, Easing.OutExpo);

                        previousText.BypassAutoSizeAxes = Axes.Both;
                        previousText.MoveToY(isIncrease ? -20 : 20, transition_duration, Easing.OutExpo).Expire();

                        AutoSizeDuration = 300;
                        AutoSizeEasing = Easing.OutQuint;
                    }
                }, true);
            }
        }
    }
}
