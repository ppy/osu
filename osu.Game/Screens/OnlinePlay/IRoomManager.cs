// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay
{
    [Cached(typeof(IRoomManager))]
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
        /// Adds a <see cref="Room"/> to this <see cref="IRoomManager"/>.
        /// If already existing, the local room will be updated with the given one.
        /// </summary>
        /// <param name="room">The incoming <see cref="Room"/>.</param>
        void AddOrUpdateRoom(Room room);

        /// <summary>
        /// Removes a <see cref="Room"/> from this <see cref="IRoomManager"/>.
        /// </summary>
        /// <param name="room">The <see cref="Room"/> to remove.</param>
        void RemoveRoom(Room room);

        /// <summary>
        /// Removes all <see cref="Room"/>s from this <see cref="IRoomManager"/>.
        /// </summary>
        void ClearRooms();

        /// <summary>
        /// Creates a new <see cref="Room"/>.
        /// </summary>
        /// <param name="room">The <see cref="Room"/> to create.</param>
        /// <param name="onSuccess">An action to be invoked if the creation succeeds.</param>
        /// <param name="onError">An action to be invoked if an error occurred.</param>
        void CreateRoom(Room room, Action<Room>? onSuccess = null, Action<string>? onError = null);

        /// <summary>
        /// Joins a <see cref="Room"/>.
        /// </summary>
        /// <param name="room">The <see cref="Room"/> to join. <see cref="Room.RoomID"/> must be populated.</param>
        /// <param name="password">An optional password to use for the join operation.</param>
        /// <param name="onSuccess"></param>
        /// <param name="onError"></param>
        void JoinRoom(Room room, string? password = null, Action<Room>? onSuccess = null, Action<string>? onError = null);

        /// <summary>
        /// Parts the currently-joined <see cref="Room"/>.
        /// </summary>
        void PartRoom();
    }
}
