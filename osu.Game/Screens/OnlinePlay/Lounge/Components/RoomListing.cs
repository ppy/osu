// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Input.Bindings;
using osu.Game.Online.Rooms;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public partial class RoomListing : CompositeDrawable, IKeyBindingHandler<GlobalAction>
    {
        /// <summary>
        /// Rooms which should be displayed. Should be managed externally.
        /// </summary>
        public readonly BindableList<Room> Rooms = new BindableList<Room>();

        /// <summary>
        /// The current filter criteria. Should be managed externally.
        /// </summary>
        public readonly Bindable<FilterCriteria?> Filter = new Bindable<FilterCriteria?>();

        /// <summary>
        /// The currently user-selected room.
        /// </summary>
        public IBindable<Room?> SelectedRoom => selectedRoom;

        private readonly Bindable<Room?> selectedRoom = new Bindable<Room?>();

        public IReadOnlyList<DrawableRoom> DrawableRooms => roomFlow.FlowingChildren.Cast<DrawableRoom>().ToArray();

        private readonly ScrollContainer<Drawable> scroll;
        private readonly FillFlowContainer<DrawableLoungeRoom> roomFlow;

        private const float display_scale = 0.8f;

        // handle deselection
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        public RoomListing()
        {
            InternalChild = scroll = new Scroll
            {
                Masking = false,
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Width = display_scale,
                ScrollbarOverlapsContent = false,
                Padding = new MarginPadding { Right = 5 },
                Child = new OsuContextMenuContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Child = roomFlow = new FillFlowContainer<DrawableLoungeRoom>
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(5),
                        Margin = new MarginPadding { Vertical = 10 },
                    }
                }
            };
        }

        private partial class Scroll : OsuScrollContainer
        {
            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;
        }

        protected override void LoadComplete()
        {
            SelectedRoom.BindValueChanged(onSelectedRoomChanged, true);
            Rooms.BindCollectionChanged(roomsChanged, true);
            Filter.BindValueChanged(criteria => applyFilterCriteria(criteria.NewValue), true);
        }

        private void applyFilterCriteria(FilterCriteria? criteria)
        {
            roomFlow.Children.ForEach(r =>
            {
                if (criteria == null)
                    r.MatchingFilter = true;
                else
                {
                    bool matchingFilter = true;

                    matchingFilter &= criteria.Ruleset == null || r.Room.CurrentPlaylistItem?.Freestyle == true || r.Room.PlaylistItemStats?.RulesetIDs.Any(id => id == criteria.Ruleset.OnlineID) != false;
                    matchingFilter &= matchPermissions(r, criteria.Permissions);

                    // Room name isn't translatable, so ToString() is used here for simplicity.
                    string[] filterTerms = r.FilterTerms.Select(t => t.ToString()).ToArray();
                    string[] searchTerms = criteria.SearchString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    matchingFilter &= searchTerms.All(searchTerm => filterTerms.Any(filterTerm => checkTerm(filterTerm, searchTerm)));

                    r.MatchingFilter = matchingFilter;
                }
            });

            // Lifted from SearchContainer.
            static bool checkTerm(string haystack, string needle)
            {
                int index = 0;

                for (int i = 0; i < needle.Length; i++)
                {
                    int found = CultureInfo.InvariantCulture.CompareInfo.IndexOf(haystack, needle[i], index, CompareOptions.OrdinalIgnoreCase);
                    if (found < 0)
                        return false;

                    index = found + 1;
                }

                return true;
            }

            static bool matchPermissions(DrawableLoungeRoom room, RoomPermissionsFilter accessType)
            {
                switch (accessType)
                {
                    case RoomPermissionsFilter.All:
                        return true;

                    case RoomPermissionsFilter.Public:
                        return !room.Room.HasPassword;

                    case RoomPermissionsFilter.Private:
                        return room.Room.HasPassword;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(accessType), accessType, $"Unsupported {nameof(RoomPermissionsFilter)} in filter");
                }
            }
        }

        private void onSelectedRoomChanged(ValueChangedEvent<Room?> room)
        {
            // scroll selected room into view on selection.
            var drawable = DrawableRooms.FirstOrDefault(r => r.Room == room.NewValue);
            if (drawable != null)
                scroll.ScrollIntoView(drawable);
        }

        private void roomsChanged(object? sender, NotifyCollectionChangedEventArgs args)
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
            {
                var drawableRoom = new DrawableLoungeRoom(room)
                {
                    SelectedRoom = selectedRoom,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(display_scale),
                    Width = 1 / display_scale,
                };

                roomFlow.Add(drawableRoom);

                // Always show spotlight playlists at the top of the listing.
                roomFlow.SetLayoutPosition(drawableRoom, room.Category > RoomCategory.Normal ? float.MinValue : -(room.RoomID ?? 0));
            }

            applyFilterCriteria(Filter.Value);
        }

        private void removeRooms(IEnumerable<Room> rooms)
        {
            foreach (var r in rooms)
            {
                roomFlow.RemoveAll(d => d.Room == r, true);

                // selection may have a lease due to being in a sub screen.
                if (SelectedRoom.Value == r && !SelectedRoom.Disabled)
                    selectedRoom.Value = null;
            }
        }

        private void clearRooms()
        {
            roomFlow.Clear();

            // selection may have a lease due to being in a sub screen.
            if (!SelectedRoom.Disabled)
                selectedRoom.Value = null;
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (!SelectedRoom.Disabled)
                selectedRoom.Value = null;
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

            var visibleRooms = DrawableRooms.AsEnumerable().Where(r => r.IsPresent);

            Room? room;

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
                selectedRoom.Value = room;
        }

        #endregion
    }
}
