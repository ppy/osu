// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Cursor;
using osu.Game.Input.Bindings;
using osu.Game.Online.Rooms;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public partial class RoomsContainer : CompositeDrawable, IKeyBindingHandler<GlobalAction>
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

                    matchingFilter &= criteria.Ruleset == null || r.Room.PlaylistItemStats.Value?.RulesetIDs.Any(id => id == criteria.Ruleset.OnlineID) != false;

                    if (!string.IsNullOrEmpty(criteria.SearchString))
                    {
                        // Room name isn't translatable, so ToString() is used here for simplicity.
                        matchingFilter &= r.FilterTerms.Any(term => term.ToString().Contains(criteria.SearchString, StringComparison.InvariantCultureIgnoreCase));
                    }

                    matchingFilter &= matchPermissions(r, criteria.Permissions);

                    r.MatchingFilter = matchingFilter;
                }
            });

            static bool matchPermissions(DrawableLoungeRoom room, RoomPermissionsFilter accessType)
            {
                switch (accessType)
                {
                    case RoomPermissionsFilter.All:
                        return true;

                    case RoomPermissionsFilter.Public:
                        return !room.Room.HasPassword.Value;

                    case RoomPermissionsFilter.Private:
                        return room.Room.HasPassword.Value;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(accessType), accessType, $"Unsupported {nameof(RoomPermissionsFilter)} in filter");
                }
            }
        }

        private void roomsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Assert(args.NewItems != null);

                    addRooms(args.NewItems.Cast<Room>());
                    break;

                case NotifyCollectionChangedAction.Remove:
                    Debug.Assert(args.OldItems != null);

                    // clear operations have a separate path that benefits from async disposal,
                    // since disposing is quite expensive when performed on a high number of drawables synchronously.
                    if (args.OldItems.Count == roomFlow.Count)
                        clearRooms();
                    else
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
                roomFlow.RemoveAll(d => d.Room == r, true);

                // selection may have a lease due to being in a sub screen.
                if (!SelectedRoom.Disabled)
                    SelectedRoom.Value = null;
            }
        }

        private void clearRooms()
        {
            roomFlow.Clear();

            // selection may have a lease due to being in a sub screen.
            if (!SelectedRoom.Disabled)
                SelectedRoom.Value = null;
        }

        private void updateSorting()
        {
            foreach (var room in roomFlow)
            {
                roomFlow.SetLayoutPosition(room, room.Room.Category.Value > RoomCategory.Normal
                    // Always show spotlight playlists at the top of the listing.
                    ? float.MinValue
                    : -(room.Room.RoomID.Value ?? 0));
            }
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (!SelectedRoom.Disabled)
                SelectedRoom.Value = null;
            return base.OnClick(e);
        }

        #region Key selection logic (shared with BeatmapCarousel and DrawableRoomPlaylist)

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
