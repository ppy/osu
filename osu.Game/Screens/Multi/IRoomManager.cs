// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Configuration;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Lounge.Components;

namespace osu.Game.Screens.Multi
{
    public interface IRoomManager
    {
        /// <summary>
        /// Invoked when a room is joined.
        /// </summary>
        event Action<Room> RoomJoined;

        /// <summary>
        /// All the active <see cref="Room"/>s.
        /// </summary>
        IBindableCollection<Room> Rooms { get; }

        /// <summary>
        /// Creates a new <see cref="Room"/>.
        /// </summary>
        /// <param name="room">The <see cref="Room"/> to create.</param>
        void CreateRoom(Room room);

        /// <summary>
        /// Joins a <see cref="Room"/>.
        /// </summary>
        /// <param name="room">The <see cref="Room"/> to join. <see cref="Room.RoomID"/> must be populated.</param>
        void JoinRoom(Room room);

        /// <summary>
        /// Parts the currently-joined <see cref="Room"/>.
        /// </summary>
        void PartRoom();

        /// <summary>
        /// Queries for <see cref="Room"/>s matching a new <see cref="FilterCriteria"/>.
        /// </summary>
        /// <param name="criteria">The <see cref="FilterCriteria"/> to match.</param>
        void Filter(FilterCriteria criteria);
    }
}
