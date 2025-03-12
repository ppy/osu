// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Ranking
{
    public partial class UserTagControl : CompositeDrawable
    {
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

        [Resolved]
        private Bindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(SessionStatics sessionStatics)
        {
            AutoSizeAxes = Axes.Y;
            InternalChildren = new Drawable[]
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
                        new AddTagsButton
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            OnTagSelected = onExtraTagSelected,
                            AvailableTags = { BindTarget = extraTags },
                        },
                    },
                },
                loadingLayer = new LoadingLayer
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

            var getBeatmapSetRequest = new GetBeatmapSetRequest(beatmap.Value.BeatmapInfo.BeatmapSet!.OnlineID);
            getBeatmapSetRequest.Success += set => apiBeatmap.Value = set.Beatmaps.SingleOrDefault(b => b.MatchesOnlineID(beatmap.Value.BeatmapInfo));
            api.Queue(getBeatmapSetRequest);
        }

        private void onExtraTagSelected(UserTag tag)
        {
            loadingLayer.Show();
            extraTags.Remove(tag);

            var req = new AddBeatmapTagRequest(beatmap.Value.BeatmapInfo.OnlineID, tag.Id);
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

        private partial class DrawableUserTag : OsuAnimatedButton
        {
            public readonly UserTag UserTag;

            private readonly Bindable<int> voteCount = new Bindable<int>();
            private readonly BindableBool voted = new BindableBool();
            private readonly Bindable<bool> confirmed = new BindableBool();

            private Box mainBackground = null!;
            private Box voteBackground = null!;
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
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
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
                        Padding = new MarginPadding { Left = 6, Right = 3, Vertical = 3, },
                        Spacing = new Vector2(5),
                        Children = new Drawable[]
                        {
                            tagNameText = new OsuSpriteText
                            {
                                Text = UserTag.Name,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                            },
                            new Container
                            {
                                AutoSizeAxes = Axes.Both,
                                CornerRadius = 5,
                                Masking = true,
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
                        voteBackground.FadeColour(colours.Lime3, transition_duration, Easing.OutQuint);
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
                        mainBackground.FadeColour(colours.Lime1, transition_duration, Easing.OutQuint);
                        tagNameText.FadeColour(Colour4.Black, transition_duration, Easing.OutQuint);
                        FadeEdgeEffectTo(0.5f, transition_duration, Easing.OutQuint);
                    }
                    else
                    {
                        mainBackground.FadeColour(colours.Gray4, transition_duration, Easing.OutQuint);
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

        private partial class AddTagsButton : GrayButton, IHasPopover
        {
            public BindableList<UserTag> AvailableTags { get; } = new BindableList<UserTag>();

            public Action<UserTag>? OnTagSelected { get; set; }

            public AddTagsButton()
                : base(FontAwesome.Solid.Plus)
            {
                Size = new Vector2(30);

                Action = this.ShowPopover;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                AvailableTags.BindCollectionChanged((_, _) => Enabled.Value = AvailableTags.Count > 0, true);
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

            public BindableList<UserTag> AvailableTags { get; } = new BindableList<UserTag>();

            public Action<UserTag>? OnSelected { get; set; }

            [BackgroundDependencyLoader]
            private void load()
            {
                Child = new OsuScrollContainer
                {
                    Width = 250,
                    Height = 250,
                    ScrollbarOverlapsContent = false,
                    Children = new Drawable[]
                    {
                        searchBox = new SearchTextBox
                        {
                            HoldFocus = true,
                            RelativeSizeAxes = Axes.X,
                        },
                        searchContainer = new SearchContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Padding = new MarginPadding { Right = 5, Top = 50, },
                            Spacing = new Vector2(10),
                            ChildrenEnumerable = AvailableTags.Select(tag => new DrawableAddableTag(tag)
                            {
                                Action = () => select(tag)
                            })
                        }
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                searchBox.Current.BindValueChanged(_ => searchContainer.SearchTerm = searchBox.Current.Value, true);
            }

            protected override bool OnKeyDown(KeyDownEvent e)
            {
                var visibleItems = searchContainer.OfType<DrawableAddableTag>().Where(d => d.IsPresent).ToArray();

                if (e.Key == Key.Enter)
                {
                    if (visibleItems.Length == 1)
                        select(visibleItems.Single().Tag);

                    return true;
                }

                return base.OnKeyDown(e);
            }

            private void select(UserTag tag)
            {
                OnSelected?.Invoke(tag);
                this.HidePopover();
            }

            private partial class DrawableAddableTag : OsuAnimatedButton, IFilterable
            {
                public readonly UserTag Tag;

                public DrawableAddableTag(UserTag tag)
                {
                    Tag = tag;

                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;
                    Anchor = Origin = Anchor.Centre;
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    Content.AddRange(new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.GreySeaFoamDark,
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
                                    Text = Tag.Name,
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

                public IEnumerable<LocalisableString> FilterTerms => [Tag.Name, Tag.Description];

                public bool MatchingFilter { set => Alpha = value ? 1 : 0; }
                public bool FilteringActive { set { } }
            }
        }
    }
}
