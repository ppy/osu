// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osuTK;

namespace osu.Game.Screens.Multi.Lounge.Components
{
    public class RoomsContainer : CompositeDrawable, IHasFilterableChildren
    {
        public Action<Room> OpenRequested;

        private readonly IBindableCollection<Room> rooms = new BindableCollection<Room>();
        private readonly Bindable<Room> currentRoom = new Bindable<Room>();

        private readonly FillFlowContainer<DrawableRoom> roomFlow;

        [Resolved]
        private RoomManager manager { get; set; }

        public RoomsContainer()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = roomFlow = new FillFlowContainer<DrawableRoom>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(2),
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            currentRoom.BindTo(manager.Current);
            rooms.BindTo(manager.Rooms);

            rooms.ItemsAdded += addRooms;
            rooms.ItemsRemoved += removeRooms;

            addRooms(rooms);

            currentRoom.BindValueChanged(selectRoom, true);
        }

        private FilterCriteria currentFilter;

        public void Filter(FilterCriteria criteria)
        {
            roomFlow.Children.ForEach(r =>
            {
                if (criteria == null)
                    r.MatchingFilter = true;
                else
                {
                    bool matchingFilter = true;
                    matchingFilter &= r.FilterTerms.Any(term => term.IndexOf(criteria.SearchString, StringComparison.InvariantCultureIgnoreCase) >= 0);

                    switch (criteria.SecondaryFilter)
                    {
                        default:
                        case SecondaryFilter.Public:
                            r.MatchingFilter = r.Room.Availability.Value == RoomAvailability.Public;
                            break;
                    }

                    r.MatchingFilter = matchingFilter;
                }
            });
            currentFilter = criteria;
        }

        private void addRooms(IEnumerable<Room> rooms)
        {
            foreach (var r in rooms)
                roomFlow.Add(new DrawableRoom(r) { Action = () => selectRoom(r) });

            Filter(currentFilter);
        }

        private void removeRooms(IEnumerable<Room> rooms)
        {
            foreach (var r in rooms)
            {
                var toRemove = roomFlow.Single(d => d.Room == r);
                toRemove.Action = null;

                roomFlow.Remove(toRemove);
            }
        }

        private void selectRoom(Room room)
        {
            var drawable = roomFlow.FirstOrDefault(r => r.Room == room);

            if (drawable != null && drawable.State == SelectionState.Selected)
                OpenRequested?.Invoke(room);
            else
            {
                currentRoom.Value = room;
                roomFlow.Children.ForEach(r => r.State = r.Room == room ? SelectionState.Selected : SelectionState.NotSelected);
            }
        }

        public IEnumerable<string> FilterTerms => Enumerable.Empty<string>();

        public IEnumerable<IFilterable> FilterableChildren => InternalChildren.OfType<IFilterable>();

        public bool MatchingFilter { set { } }
    }
}
