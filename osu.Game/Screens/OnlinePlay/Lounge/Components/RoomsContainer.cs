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
using osu.Framework.Input.Events;
using osu.Game.Extensions;
using osu.Game.Graphics.Cursor;
using osu.Game.Input.Bindings;
using osu.Game.Online.Rooms;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public class RoomsContainer : CompositeDrawable, IKeyBindingHandler<GlobalAction>
    {
        public readonly Bindable<Room> SelectedRoom = new Bindable<Room>();
        public readonly Bindable<FilterCriteria> Filter = new Bindable<FilterCriteria>();

        public IReadOnlyList<DrawableRoom> Rooms => roomFlow.FlowingChildren.Cast<DrawableRoom>().ToArray();

        private readonly IBindableList<Room> rooms = new BindableList<Room>();
        private readonly FillFlowContainer<DrawableLoungeRoom> roomFlow;

        [Resolved]
        private IRoomManager roomManager { get; set; }

        // handle deselection
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        public RoomsContainer()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            // account for the fact we are in a scroll container and want a bit of spacing from the scroll bar.
            Padding = new MarginPadding { Right = 5 };

            InternalChild = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Child = roomFlow = new FillFlowContainer<DrawableLoungeRoom>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                }
            };
        }

        protected override void LoadComplete()
        {
            rooms.CollectionChanged += roomsChanged;
            roomManager.RoomsUpdated += updateSorting;

            rooms.BindTo(roomManager.Rooms);

            Filter?.BindValueChanged(criteria => applyFilterCriteria(criteria.NewValue), true);
        }

        private void applyFilterCriteria(FilterCriteria criteria)
        {
            roomFlow.Children.ForEach(r =>
            {
                if (criteria == null)
                    r.MatchingFilter = true;
                else
                {
                    bool matchingFilter = true;

                    matchingFilter &= r.Room.Playlist.Count == 0 || criteria.Ruleset == null || r.Room.Playlist.Any(i => i.Ruleset.Value.MatchesOnlineID(criteria.Ruleset));

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
                roomFlow.Add(new DrawableLoungeRoom(room) { SelectedRoom = { BindTarget = SelectedRoom } });

            applyFilterCriteria(Filter?.Value);
        }

        private void removeRooms(IEnumerable<Room> rooms)
        {
            foreach (var r in rooms)
            {
                roomFlow.RemoveAll(d => d.Room == r);

                // selection may have a lease due to being in a sub screen.
                if (!SelectedRoom.Disabled)
                    SelectedRoom.Value = null;
            }
        }

        private void updateSorting()
        {
            foreach (var room in roomFlow)
                roomFlow.SetLayoutPosition(room, -(room.Room.RoomID.Value ?? 0));
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (!SelectedRoom.Disabled)
                SelectedRoom.Value = null;
            return base.OnClick(e);
        }

        #region Key selection logic (shared with BeatmapCarousel)

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.SelectNext:
                    selectNext(1);
                    return true;

                case GlobalAction.SelectPrevious:
                    selectNext(-1);
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        private void selectNext(int direction)
        {
            if (SelectedRoom.Disabled)
                return;

            var visibleRooms = Rooms.AsEnumerable().Where(r => r.IsPresent);

            Room room;

            if (SelectedRoom.Value == null)
                room = visibleRooms.FirstOrDefault()?.Room;
            else
            {
                if (direction < 0)
                    visibleRooms = visibleRooms.Reverse();

                room = visibleRooms.SkipWhile(r => r.Room != SelectedRoom.Value).Skip(1).FirstOrDefault()?.Room;
            }

            // we already have a valid selection only change selection if we still have a room to switch to.
            if (room != null)
                SelectedRoom.Value = room;
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
