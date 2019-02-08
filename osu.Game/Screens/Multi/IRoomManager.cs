// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Configuration;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Lounge.Components;

namespace osu.Game.Screens.Multi
{
    public interface IRoomManager
    {
        /// <summary>
        /// Invoked when the <see cref="Room"/>s have been updated.
        /// </summary>
        event Action RoomsUpdated;

        /// <summary>
        /// All the active <see cref="Room"/>s.
        /// </summary>
        IBindableList<Room> Rooms { get; }

        /// <summary>
        /// Creates a new <see cref="Room"/>.
        /// </summary>
        /// <param name="room">The <see cref="Room"/> to create.</param>
        /// <param name="onSuccess">An action to be invoked if the creation succeeds.</param>
        /// <param name="onError">An action to be invoked if an error occurred.</param>
        void CreateRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null);

        /// <summary>
        /// Joins a <see cref="Room"/>.
        /// </summary>
        /// <param name="room">The <see cref="Room"/> to join. <see cref="Room.RoomID"/> must be populated.</param>
        /// <param name="onSuccess"></param>
        /// <param name="onError"></param>
        void JoinRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null);

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
