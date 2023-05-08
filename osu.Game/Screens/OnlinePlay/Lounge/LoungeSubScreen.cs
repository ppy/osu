﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Lounge
{
    [Cached]
    public abstract partial class LoungeSubScreen : OnlinePlaySubScreen
    {
        public override string Title => "Lounge";

        protected override bool PlayExitSound => false;

        protected override BackgroundScreen CreateBackground() => new LoungeBackgroundScreen
        {
            SelectedRoom = { BindTarget = SelectedRoom }
        };

        protected override UserActivity InitialActivity => new UserActivity.SearchingForLobby();

        protected Container<OsuButton> Buttons { get; } = new Container<OsuButton>
        {
            Anchor = Anchor.BottomLeft,
            Origin = Anchor.BottomLeft,
            AutoSizeAxes = Axes.Both
        };

        protected ListingPollingComponent ListingPollingComponent { get; private set; }

        protected readonly Bindable<Room> SelectedRoom = new Bindable<Room>();

        [Resolved]
        private MusicController music { get; set; }

        [Resolved(CanBeNull = true)]
        private OngoingOperationTracker ongoingOperationTracker { get; set; }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        [CanBeNull]
        private IDisposable joiningRoomOperation { get; set; }

        [CanBeNull]
        private LeasedBindable<Room> selectionLease;

        private readonly Bindable<FilterCriteria> filter = new Bindable<FilterCriteria>(new FilterCriteria());
        private readonly IBindable<bool> operationInProgress = new Bindable<bool>();
        private readonly IBindable<bool> isIdle = new BindableBool();
        private PopoverContainer popoverContainer;
        private LoadingLayer loadingLayer;
        private RoomsContainer roomsContainer;
        private SearchTextBox searchTextBox;
        private Dropdown<RoomStatusFilter> statusDropdown;

        [BackgroundDependencyLoader(true)]
        private void load([CanBeNull] IdleTracker idleTracker)
        {
            const float controls_area_height = 25f;

            if (idleTracker != null)
                isIdle.BindTo(idleTracker.IsIdle);

            OsuScrollContainer scrollContainer;

            InternalChildren = new Drawable[]
            {
                ListingPollingComponent = CreatePollingComponent().With(c => c.Filter.BindTarget = filter),
                popoverContainer = new PopoverContainer
                {
                    Name = @"Rooms area",
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding
                    {
                        Horizontal = WaveOverlayContainer.WIDTH_PADDING,
                        Top = Header.HEIGHT + controls_area_height + 20,
                    },
                    Child = scrollContainer = new OsuScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        ScrollbarOverlapsContent = false,
                        Child = roomsContainer = new RoomsContainer
                        {
                            Filter = { BindTarget = filter },
                            SelectedRoom = { BindTarget = SelectedRoom }
                        }
                    },
                },
                loadingLayer = new LoadingLayer(true),
                new FillFlowContainer
                {
                    Name = @"Header area flow",
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = WaveOverlayContainer.WIDTH_PADDING },
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = Header.HEIGHT,
                            Child = searchTextBox = new BasicSearchTextBox
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                RelativeSizeAxes = Axes.X,
                                Width = 0.6f,
                            },
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = controls_area_height,
                            Children = new Drawable[]
                            {
                                Buttons.WithChild(CreateNewRoomButton().With(d =>
                                {
                                    d.Anchor = Anchor.BottomLeft;
                                    d.Origin = Anchor.BottomLeft;
                                    d.Size = new Vector2(150, 37.5f);
                                    d.Action = () => Open();
                                })),
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(10),
                                    ChildrenEnumerable = CreateFilterControls().Select(f => f.With(d =>
                                    {
                                        d.Anchor = Anchor.TopRight;
                                        d.Origin = Anchor.TopRight;
                                    }))
                                }
                            }
                        }
                    },
                },
            };

            // scroll selected room into view on selection.
            SelectedRoom.BindValueChanged(val =>
            {
                var drawable = roomsContainer.Rooms.FirstOrDefault(r => r.Room == val.NewValue);
                if (drawable != null)
                    scrollContainer.ScrollIntoView(drawable);
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            searchTextBox.Current.BindValueChanged(_ => updateFilterDebounced());
            ruleset.BindValueChanged(_ => UpdateFilter());

            isIdle.BindValueChanged(_ => updatePollingRate(this.IsCurrentScreen()), true);

            if (ongoingOperationTracker != null)
            {
                operationInProgress.BindTo(ongoingOperationTracker.InProgress);
                operationInProgress.BindValueChanged(_ => updateLoadingLayer());
            }

            ListingPollingComponent.InitialRoomsReceived.BindValueChanged(_ => updateLoadingLayer(), true);

            updateFilter();
        }

        #region Filtering

        public void UpdateFilter() => Scheduler.AddOnce(updateFilter);

        private ScheduledDelegate scheduledFilterUpdate;

        private void updateFilterDebounced()
        {
            scheduledFilterUpdate?.Cancel();
            scheduledFilterUpdate = Scheduler.AddDelayed(UpdateFilter, 200);
        }

        private void updateFilter()
        {
            scheduledFilterUpdate?.Cancel();
            filter.Value = CreateFilterCriteria();
        }

        protected virtual FilterCriteria CreateFilterCriteria() => new FilterCriteria
        {
            SearchString = searchTextBox.Current.Value,
            Ruleset = ruleset.Value,
            Status = statusDropdown.Current.Value
        };

        protected virtual IEnumerable<Drawable> CreateFilterControls()
        {
            statusDropdown = new SlimEnumDropdown<RoomStatusFilter>
            {
                RelativeSizeAxes = Axes.None,
                Width = 160,
            };

            statusDropdown.Current.BindValueChanged(_ => UpdateFilter());

            yield return statusDropdown;
        }

        #endregion

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);
            onReturning();
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            Debug.Assert(selectionLease != null);

            selectionLease.Return();
            selectionLease = null;

            if (SelectedRoom.Value?.RoomID.Value == null)
                SelectedRoom.Value = new Room();

            music?.EnsurePlayingSomething();

            onReturning();
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            onLeaving();
            return base.OnExiting(e);
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            onLeaving();
            base.OnSuspending(e);
        }

        protected override void OnFocus(FocusEvent e)
        {
            searchTextBox.TakeFocus();
        }

        private void onReturning()
        {
            updatePollingRate(true);
            searchTextBox.HoldFocus = true;
        }

        private void onLeaving()
        {
            updatePollingRate(false);
            searchTextBox.HoldFocus = false;

            // ensure any password prompt is dismissed.
            popoverContainer.HidePopover();
        }

        public virtual void Join(Room room, string password, Action<Room> onSuccess = null, Action<string> onFailure = null) => Schedule(() =>
        {
            if (joiningRoomOperation != null)
                return;

            joiningRoomOperation = ongoingOperationTracker?.BeginOperation();

            RoomManager?.JoinRoom(room, password, _ =>
            {
                Open(room);
                joiningRoomOperation?.Dispose();
                joiningRoomOperation = null;
                onSuccess?.Invoke(room);
            }, error =>
            {
                joiningRoomOperation?.Dispose();
                joiningRoomOperation = null;
                onFailure?.Invoke(error);
            });
        });

        /// <summary>
        /// Copies a room and opens it as a fresh (not-yet-created) one.
        /// </summary>
        /// <param name="room">The room to copy.</param>
        public void OpenCopy(Room room)
        {
            Debug.Assert(room.RoomID.Value != null);

            if (joiningRoomOperation != null)
                return;

            joiningRoomOperation = ongoingOperationTracker?.BeginOperation();

            var req = new GetRoomRequest(room.RoomID.Value.Value);

            req.Success += r =>
            {
                // ID must be unset as we use this as a marker for whether this is a client-side (not-yet-created) room or not.
                r.RoomID.Value = null;

                // Null out dates because end date is not supported client-side and the settings overlay will populate a duration.
                r.EndDate.Value = null;
                r.Duration.Value = null;

                Open(r);

                joiningRoomOperation?.Dispose();
                joiningRoomOperation = null;
            };

            req.Failure += exception =>
            {
                Logger.Error(exception, "Couldn't create a copy of this room.");
                joiningRoomOperation?.Dispose();
                joiningRoomOperation = null;
            };

            api.Queue(req);
        }

        /// <summary>
        /// Push a room as a new subscreen.
        /// </summary>
        /// <param name="room">An optional template to use when creating the room.</param>
        public void Open(Room room = null) => Schedule(() =>
        {
            // Handles the case where a room is clicked 3 times in quick succession
            if (!this.IsCurrentScreen())
                return;

            OpenNewRoom(room ?? CreateNewRoom());
        });

        protected virtual void OpenNewRoom(Room room)
        {
            selectionLease = SelectedRoom.BeginLease(false);
            Debug.Assert(selectionLease != null);
            selectionLease.Value = room;

            this.Push(CreateRoomSubScreen(room));
        }

        private void updateLoadingLayer()
        {
            if (operationInProgress.Value || !ListingPollingComponent.InitialRoomsReceived.Value)
                loadingLayer.Show();
            else
                loadingLayer.Hide();
        }

        private void updatePollingRate(bool isCurrentScreen)
        {
            if (!isCurrentScreen)
                ListingPollingComponent.TimeBetweenPolls.Value = 0;
            else
                ListingPollingComponent.TimeBetweenPolls.Value = isIdle.Value ? 120000 : 15000;

            Logger.Log($"Polling adjusted (listing: {ListingPollingComponent.TimeBetweenPolls.Value})");
        }

        protected abstract OsuButton CreateNewRoomButton();

        /// <summary>
        /// Creates a new room.
        /// </summary>
        /// <returns>The created <see cref="Room"/>.</returns>
        protected abstract Room CreateNewRoom();

        protected abstract RoomSubScreen CreateRoomSubScreen(Room room);

        protected abstract ListingPollingComponent CreatePollingComponent();
    }
}
