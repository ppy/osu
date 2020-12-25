// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Threading;
using osu.Game.Extensions;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Online.Rooms;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public class RoomsContainer : CompositeDrawable, IKeyBindingHandler<GlobalAction>
    {
        public Action<Room> JoinRequested;

        private readonly IBindableList<Room> rooms = new BindableList<Room>();

        private readonly FillFlowContainer<DrawableRoom> roomFlow;
        public IReadOnlyList<DrawableRoom> Rooms => roomFlow;

        [Resolved(CanBeNull = true)]
        private Bindable<FilterCriteria> filter { get; set; }

        [Resolved]
        private Bindable<Room> selectedRoom { get; set; }

        [Resolved]
        private IRoomManager roomManager { get; set; }

        [Resolved(CanBeNull = true)]
        private LoungeSubScreen loungeSubScreen { get; set; }

        public RoomsContainer()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Child = roomFlow = new FillFlowContainer<DrawableRoom>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(2),
                }
            };
        }

        protected override void LoadComplete()
        {
            rooms.CollectionChanged += roomsChanged;
            roomManager.RoomsUpdated += updateSorting;

            rooms.BindTo(roomManager.Rooms);

            filter?.BindValueChanged(criteria => Filter(criteria.NewValue));
        }

        public void Filter(FilterCriteria criteria)
        {
            roomFlow.Children.ForEach(r =>
            {
                if (criteria == null)
                    r.MatchingFilter = true;
                else
                {
                    bool matchingFilter = true;

                    matchingFilter &= r.Room.Playlist.Count == 0 || r.Room.Playlist.Any(i => i.Ruleset.Value.Equals(criteria.Ruleset));

                    if (!string.IsNullOrEmpty(criteria.SearchString))
                        matchingFilter &= r.FilterTerms.Any(term => term.Contains(criteria.SearchString, StringComparison.InvariantCultureIgnoreCase));

                    r.MatchingFilter = matchingFilter;
                }
            });
        }

        private void roomsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    addRooms(args.NewItems.Cast<Room>());
                    break;

                case NotifyCollectionChangedAction.Remove:
                    removeRooms(args.OldItems.Cast<Room>());
                    break;
            }
        }

        private void addRooms(IEnumerable<Room> rooms)
        {
            foreach (var room in rooms)
            {
                roomFlow.Add(new DrawableRoom(room)
                {
                    Action = () =>
                    {
                        if (room == selectedRoom.Value)
                        {
                            joinSelected();
                            return;
                        }

                        selectRoom(room);
                    }
                });
            }

            Filter(filter?.Value);
        }

        private void removeRooms(IEnumerable<Room> rooms)
        {
            foreach (var r in rooms)
            {
                var toRemove = roomFlow.Single(d => d.Room == r);
                toRemove.Action = null;

                roomFlow.Remove(toRemove);

                selectRoom(null);
            }
        }

        private void updateSorting()
        {
            foreach (var room in roomFlow)
                roomFlow.SetLayoutPosition(room, room.Room.Position.Value);
        }

        private void selectRoom(Room room)
        {
            roomFlow.Children.ForEach(r => r.State = r.Room == room ? SelectionState.Selected : SelectionState.NotSelected);
            selectedRoom.Value = room;
        }

        private void joinSelected()
        {
            if (selectedRoom.Value == null) return;

            JoinRequested?.Invoke(selectedRoom.Value);
        }

        #region Key selection logic (shared with BeatmapCarousel)

        public bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.Select:
                    joinSelected();
                    return true;

                case GlobalAction.SelectNext:
                    beginRepeatSelection(() => selectNext(1), action);
                    return true;

                case GlobalAction.SelectPrevious:
                    beginRepeatSelection(() => selectNext(-1), action);
                    return true;
            }

            return false;
        }

        public void OnReleased(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.SelectNext:
                case GlobalAction.SelectPrevious:
                    endRepeatSelection(action);
                    break;
            }
        }

        private ScheduledDelegate repeatDelegate;
        private object lastRepeatSource;

        /// <summary>
        /// Begin repeating the specified selection action.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="source">The source of the action. Used in conjunction with <see cref="endRepeatSelection"/> to only cancel the correct action (most recently pressed key).</param>
        private void beginRepeatSelection(Action action, object source)
        {
            endRepeatSelection();

            lastRepeatSource = source;
            repeatDelegate = this.BeginKeyRepeat(Scheduler, action);
        }

        private void endRepeatSelection(object source = null)
        {
            // only the most recent source should be able to cancel the current action.
            if (source != null && !EqualityComparer<object>.Default.Equals(lastRepeatSource, source))
                return;

            repeatDelegate?.Cancel();
            repeatDelegate = null;
            lastRepeatSource = null;
        }

        private void selectNext(int direction)
        {
            var visibleRooms = Rooms.AsEnumerable().Where(r => r.IsPresent);

            Room room;

            if (selectedRoom.Value == null)
                room = visibleRooms.FirstOrDefault()?.Room;
            else
            {
                if (direction < 0)
                    visibleRooms = visibleRooms.Reverse();

                room = visibleRooms.SkipWhile(r => r.Room != selectedRoom.Value).Skip(1).FirstOrDefault()?.Room;
            }

            // we already have a valid selection only change selection if we still have a room to switch to.
            if (room != null)
                selectRoom(room);
        }

        #endregion

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (roomManager != null)
                roomManager.RoomsUpdated -= updateSorting;
        }
    }
}
