// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Lounge
{
    [Cached]
    public abstract class LoungeSubScreen : OnlinePlaySubScreen
    {
        public override string Title => "Lounge";

        protected override UserActivity InitialActivity => new UserActivity.SearchingForLobby();

        protected Container<OsuButton> Buttons { get; } = new Container<OsuButton>
        {
            Anchor = Anchor.BottomLeft,
            Origin = Anchor.BottomLeft,
            AutoSizeAxes = Axes.Both
        };

        private readonly IBindable<bool> initialRoomsReceived = new Bindable<bool>();
        private readonly IBindable<bool> operationInProgress = new Bindable<bool>();

        private LoadingLayer loadingLayer;

        [Resolved]
        private Bindable<Room> selectedRoom { get; set; }

        [Resolved]
        private MusicController music { get; set; }

        [Resolved(CanBeNull = true)]
        private OngoingOperationTracker ongoingOperationTracker { get; set; }

        [Resolved(CanBeNull = true)]
        private Bindable<FilterCriteria> filter { get; set; }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        [CanBeNull]
        private IDisposable joiningRoomOperation { get; set; }

        private RoomsContainer roomsContainer;
        private SearchTextBox searchTextBox;
        private Dropdown<RoomStatusFilter> statusDropdown;

        [CanBeNull]
        private LeasedBindable<Room> selectionLease;

        [BackgroundDependencyLoader]
        private void load()
        {
            filter ??= new Bindable<FilterCriteria>(new FilterCriteria());

            OsuScrollContainer scrollContainer;

            InternalChildren = new[]
            {
                loadingLayer = new LoadingLayer(true),
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding
                    {
                        Left = WaveOverlayContainer.WIDTH_PADDING,
                        Right = WaveOverlayContainer.WIDTH_PADDING,
                    },
                    Child = new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.Absolute, Header.HEIGHT),
                            new Dimension(GridSizeMode.Absolute, 25),
                            new Dimension(GridSizeMode.Absolute, 20)
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                searchTextBox = new LoungeSearchTextBox
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    RelativeSizeAxes = Axes.X,
                                    Width = 0.6f,
                                },
                            },
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Depth = float.MinValue, // Contained filters should appear over the top of rooms.
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
                            null,
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        scrollContainer = new OsuScrollContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            ScrollbarOverlapsContent = false,
                                            Child = roomsContainer = new RoomsContainer()
                                        },
                                    }
                                },
                            }
                        }
                    },
                },
            };

            // scroll selected room into view on selection.
            selectedRoom.BindValueChanged(val =>
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

            initialRoomsReceived.BindTo(RoomManager.InitialRoomsReceived);
            initialRoomsReceived.BindValueChanged(_ => updateLoadingLayer());

            if (ongoingOperationTracker != null)
            {
                operationInProgress.BindTo(ongoingOperationTracker.InProgress);
                operationInProgress.BindValueChanged(_ => updateLoadingLayer(), true);
            }

            updateFilter();
        }

        #region Filtering

        protected void UpdateFilter() => Scheduler.AddOnce(updateFilter);

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

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            onReturning();
        }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);

            Debug.Assert(selectionLease != null);

            selectionLease.Return();
            selectionLease = null;

            if (selectedRoom.Value?.RoomID.Value == null)
                selectedRoom.Value = new Room();

            music?.EnsurePlayingSomething();

            onReturning();
        }

        public override bool OnExiting(IScreen next)
        {
            onLeaving();
            return base.OnExiting(next);
        }

        public override void OnSuspending(IScreen next)
        {
            onLeaving();
            base.OnSuspending(next);
        }

        protected override void OnFocus(FocusEvent e)
        {
            searchTextBox.TakeFocus();
        }

        private void onReturning()
        {
            searchTextBox.HoldFocus = true;
        }

        private void onLeaving()
        {
            searchTextBox.HoldFocus = false;

            // ensure any password prompt is dismissed.
            this.HidePopover();
        }

        public void Join(Room room, string password) => Schedule(() =>
        {
            if (joiningRoomOperation != null)
                return;

            joiningRoomOperation = ongoingOperationTracker?.BeginOperation();

            RoomManager?.JoinRoom(room, password, r =>
            {
                Open(room);
                joiningRoomOperation?.Dispose();
                joiningRoomOperation = null;
            }, _ =>
            {
                joiningRoomOperation?.Dispose();
                joiningRoomOperation = null;
            });
        });

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
            selectionLease = selectedRoom.BeginLease(false);
            Debug.Assert(selectionLease != null);
            selectionLease.Value = room;

            this.Push(CreateRoomSubScreen(room));
        }

        protected abstract OsuButton CreateNewRoomButton();

        /// <summary>
        /// Creates a new room.
        /// </summary>
        /// <returns>The created <see cref="Room"/>.</returns>
        protected abstract Room CreateNewRoom();

        protected abstract RoomSubScreen CreateRoomSubScreen(Room room);

        private void updateLoadingLayer()
        {
            if (operationInProgress.Value || !initialRoomsReceived.Value)
                loadingLayer.Show();
            else
                loadingLayer.Hide();
        }

        private class LoungeSearchTextBox : SearchTextBox
        {
            [BackgroundDependencyLoader]
            private void load()
            {
                BackgroundUnfocused = OsuColour.Gray(0.06f);
                BackgroundFocused = OsuColour.Gray(0.12f);
            }
        }
    }
}
