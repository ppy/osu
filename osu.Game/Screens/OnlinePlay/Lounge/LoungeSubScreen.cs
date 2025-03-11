// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Lounge
{
    [Cached]
    [Cached(typeof(IOnlinePlayLounge))]
    public abstract partial class LoungeSubScreen : OnlinePlaySubScreen, IOnlinePlayLounge
    {
        public override string Title => "Lounge";

        protected override BackgroundScreen CreateBackground() => new LoungeBackgroundScreen
        {
            SelectedRoom = { BindTarget = roomListing.SelectedRoom }
        };

        protected override UserActivity InitialActivity => new UserActivity.SearchingForLobby();

        protected Container<OsuButton> Buttons { get; } = new Container<OsuButton>
        {
            Anchor = Anchor.BottomLeft,
            Origin = Anchor.BottomLeft,
            AutoSizeAxes = Axes.Both
        };

        [Resolved]
        private MusicController music { get; set; } = null!;

        [Resolved(CanBeNull = true)]
        private OngoingOperationTracker? ongoingOperationTracker { get; set; }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved(CanBeNull = true)]
        private IdleTracker? idleTracker { get; set; }

        [Resolved]
        protected OsuConfigManager Config { get; private set; } = null!;

        private IDisposable? joiningRoomOperation;

        private readonly Bindable<FilterCriteria?> filter = new Bindable<FilterCriteria?>();
        private readonly Bindable<bool> hasListingResults = new Bindable<bool>();
        private readonly IBindable<bool> operationInProgress = new Bindable<bool>();
        private readonly IBindable<bool> isIdle = new BindableBool();
        private RoomListing roomListing = null!;
        private LoungeListingPoller listingPoller = null!;
        private PopoverContainer popoverContainer = null!;
        private LoadingLayer loadingLayer = null!;
        private SearchTextBox searchTextBox = null!;

        protected Dropdown<RoomModeFilter> StatusDropdown { get; private set; } = null!;

        [BackgroundDependencyLoader(true)]
        private void load()
        {
            Masking = true;

            const float controls_area_height = 25f;

            if (idleTracker != null)
                isIdle.BindTo(idleTracker.IsIdle);

            Color4 bg = Color4Extensions.FromHex("#070405");

            InternalChildren = new Drawable[]
            {
                listingPoller = new LoungeListingPoller
                {
                    RoomsReceived = onListingReceived,
                    Filter = { BindTarget = filter }
                },
                popoverContainer = new PopoverContainer
                {
                    Name = @"Rooms area",
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding
                    {
                        Horizontal = WaveOverlayContainer.WIDTH_PADDING,
                        Top = Header.HEIGHT + controls_area_height + 20,
                    },
                    Child = roomListing = new RoomListing
                    {
                        RelativeSizeAxes = Axes.Both,
                        Filter = { BindTarget = filter },
                    }
                },
                loadingLayer = new LoadingLayer(true),
                new Container
                {
                    Name = "Header area",
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = ColourInfo.GradientVertical(bg, bg.Opacity(0.75f)),
                            RelativeSizeAxes = Axes.Both,
                            Height = 0.8f,
                        },
                        new Box
                        {
                            Colour = ColourInfo.GradientVertical(bg.Opacity(0.75f), bg.Opacity(0)),
                            RelativeSizeAxes = Axes.Both,
                            RelativePositionAxes = Axes.Both,
                            Y = 0.8f,
                            // Intentionally taller than the header for a more gradual fade
                            Height = 0.5f,
                        },
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
                    }
                },
            };
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

            hasListingResults.BindValueChanged(_ => updateLoadingLayer());

            filter.BindValueChanged(_ =>
            {
                roomListing.Rooms.Clear();
                RefreshRooms();
            });

            updateLoadingLayer();
            updateFilter();
        }

        private void onListingReceived(Room[] result)
        {
            Dictionary<long, Room> localRoomsById = roomListing.Rooms.ToDictionary(r => r.RoomID!.Value);
            Dictionary<long, Room> resultRoomsById = result.ToDictionary(r => r.RoomID!.Value);

            // Remove all local rooms no longer in the result set.
            roomListing.Rooms.RemoveAll(r => !resultRoomsById.ContainsKey(r.RoomID!.Value));

            // Add or update local rooms with the result set.
            foreach (var r in result)
            {
                if (localRoomsById.TryGetValue(r.RoomID!.Value, out Room? existingRoom))
                    existingRoom.CopyFrom(r);
                else
                    roomListing.Rooms.Add(r);
            }

            hasListingResults.Value = true;
        }

        #region Filtering

        public void UpdateFilter() => Scheduler.AddOnce(updateFilter);

        private ScheduledDelegate? scheduledFilterUpdate;

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
            Mode = StatusDropdown.Current.Value
        };

        protected virtual IEnumerable<Drawable> CreateFilterControls()
        {
            StatusDropdown = new SlimEnumDropdown<RoomModeFilter>
            {
                RelativeSizeAxes = Axes.None,
                Width = 160,
            };

            StatusDropdown.Current.BindValueChanged(_ => UpdateFilter());

            yield return StatusDropdown;
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

            music.EnsurePlayingSomething();

            onReturning();

            // Poll for any newly-created rooms (including potentially the user's own).
            listingPoller.PollImmediately();
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

        public void Join(Room room, string? password, Action<Room>? onSuccess = null, Action<string>? onFailure = null) => Schedule(() =>
        {
            if (joiningRoomOperation != null)
                return;

            joiningRoomOperation = ongoingOperationTracker?.BeginOperation();

            JoinInternal(room, password, r =>
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

        protected abstract void JoinInternal(Room room, string? password, Action<Room> onSuccess, Action<string> onFailure);

        public void OpenCopy(Room room)
        {
            Debug.Assert(room.RoomID != null);

            if (joiningRoomOperation != null)
                return;

            joiningRoomOperation = ongoingOperationTracker?.BeginOperation();

            var req = new GetRoomRequest(room.RoomID.Value);

            req.Success += r =>
            {
                // ID must be unset as we use this as a marker for whether this is a client-side (not-yet-created) room or not.
                r.RoomID = null;

                // Null out dates because end date is not supported client-side and the settings overlay will populate a duration.
                r.EndDate = null;
                r.Duration = null;

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

        public abstract void Close(Room room);

        /// <summary>
        /// Push a room as a new subscreen.
        /// </summary>
        /// <param name="room">An optional template to use when creating the room.</param>
        public void Open(Room? room = null) => Schedule(() =>
        {
            // Handles the case where a room is clicked 3 times in quick succession
            if (!this.IsCurrentScreen())
                return;

            OpenNewRoom(room ?? CreateNewRoom());
        });

        protected virtual void OpenNewRoom(Room room) => this.Push(CreateRoomSubScreen(room));

        public void RefreshRooms()
        {
            hasListingResults.Value = false;
            listingPoller.PollImmediately();
        }

        private void updateLoadingLayer()
        {
            if (operationInProgress.Value || !hasListingResults.Value)
                loadingLayer.Show();
            else
                loadingLayer.Hide();
        }

        private void updatePollingRate(bool isCurrentScreen)
        {
            if (!isCurrentScreen)
                listingPoller.TimeBetweenPolls.Value = 0;
            else
                listingPoller.TimeBetweenPolls.Value = isIdle.Value ? 120000 : 15000;

            Logger.Log($"Polling adjusted (listing: {listingPoller.TimeBetweenPolls.Value})");
        }

        protected abstract OsuButton CreateNewRoomButton();

        /// <summary>
        /// Creates a new room.
        /// </summary>
        /// <returns>The created <see cref="Room"/>.</returns>
        protected abstract Room CreateNewRoom();

        protected abstract OnlinePlaySubScreen CreateRoomSubScreen(Room room);
    }
}
