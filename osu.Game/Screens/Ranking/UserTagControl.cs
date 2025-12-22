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
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
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

        private AddNewTagUserTag? addNewTagUserTag;

        /// <summary>
        /// Determines whether the user can modify the contained tags
        /// </summary>
        public bool Writable { private get; init; }

        private InputManager inputManager = null!;

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
                                        Children = Writable
                                            ?
                                            [
                                                addNewTagUserTag = new AddNewTagUserTag
                                                {
                                                    AvailableTags = { BindTarget = relevantTagsById },
                                                    OnTagSelected = toggleVote,
                                                }
                                            ]
                                            : []
                                    }
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

            inputManager = GetContainingInputManager()!;
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
                    if (Writable) tagFlow.Add(addNewTagUserTag!);
                    break;
                }
            }
        }

        private void toggleVote(UserTag tag)
        {
            if (!Writable)
                return;

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

            if (!layout.IsValid && !Contains(inputManager.CurrentState.Mouse.Position))
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
    }
}
