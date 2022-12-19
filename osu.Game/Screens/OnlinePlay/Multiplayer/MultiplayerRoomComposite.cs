// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public abstract partial class MultiplayerRoomComposite : OnlinePlayComposite
    {
        [CanBeNull]
        protected MultiplayerRoom Room => Client.Room;

        [Resolved]
        protected MultiplayerClient Client { get; private set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Client.RoomUpdated += invokeOnRoomUpdated;
            Client.LoadRequested += invokeOnRoomLoadRequested;
            Client.UserLeft += invokeUserLeft;
            Client.UserKicked += invokeUserKicked;
            Client.UserJoined += invokeUserJoined;
            Client.ItemAdded += invokeItemAdded;
            Client.ItemRemoved += invokeItemRemoved;
            Client.ItemChanged += invokeItemChanged;

            OnRoomUpdated();
        }

        private void invokeOnRoomUpdated() => Scheduler.AddOnce(OnRoomUpdated);
        private void invokeUserJoined(MultiplayerRoomUser user) => Scheduler.Add(() => UserJoined(user));
        private void invokeUserKicked(MultiplayerRoomUser user) => Scheduler.Add(() => UserKicked(user));
        private void invokeUserLeft(MultiplayerRoomUser user) => Scheduler.Add(() => UserLeft(user));
        private void invokeItemAdded(MultiplayerPlaylistItem item) => Schedule(() => PlaylistItemAdded(item));
        private void invokeItemRemoved(long item) => Schedule(() => PlaylistItemRemoved(item));
        private void invokeItemChanged(MultiplayerPlaylistItem item) => Schedule(() => PlaylistItemChanged(item));
        private void invokeOnRoomLoadRequested() => Scheduler.AddOnce(OnRoomLoadRequested);

        /// <summary>
        /// Invoked when a user has joined the room.
        /// </summary>
        /// <param name="user">The user.</param>
        protected virtual void UserJoined(MultiplayerRoomUser user)
        {
        }

        /// <summary>
        /// Invoked when a user has been kicked from the room (including the local user).
        /// </summary>
        /// <param name="user">The user.</param>
        protected virtual void UserKicked(MultiplayerRoomUser user)
        {
        }

        /// <summary>
        /// Invoked when a user has left the room.
        /// </summary>
        /// <param name="user">The user.</param>
        protected virtual void UserLeft(MultiplayerRoomUser user)
        {
        }

        /// <summary>
        /// Invoked when a playlist item is added to the room.
        /// </summary>
        /// <param name="item">The added playlist item.</param>
        protected virtual void PlaylistItemAdded(MultiplayerPlaylistItem item)
        {
        }

        /// <summary>
        /// Invoked when a playlist item is removed from the room.
        /// </summary>
        /// <param name="item">The ID of the removed playlist item.</param>
        protected virtual void PlaylistItemRemoved(long item)
        {
        }

        /// <summary>
        /// Invoked when a playlist item is changed in the room.
        /// </summary>
        /// <param name="item">The new playlist item, with an existing item's ID.</param>
        protected virtual void PlaylistItemChanged(MultiplayerPlaylistItem item)
        {
        }

        /// <summary>
        /// Invoked when any change occurs to the multiplayer room.
        /// </summary>
        protected virtual void OnRoomUpdated()
        {
        }

        /// <summary>
        /// Invoked when the room requests the local user to load into gameplay.
        /// </summary>
        protected virtual void OnRoomLoadRequested()
        {
        }

        protected override void Dispose(bool isDisposing)
        {
            if (Client != null)
            {
                Client.RoomUpdated -= invokeOnRoomUpdated;
                Client.LoadRequested -= invokeOnRoomLoadRequested;
                Client.UserLeft -= invokeUserLeft;
                Client.UserKicked -= invokeUserKicked;
                Client.UserJoined -= invokeUserJoined;
                Client.ItemAdded -= invokeItemAdded;
                Client.ItemRemoved -= invokeItemRemoved;
                Client.ItemChanged -= invokeItemChanged;
            }

            base.Dispose(isDisposing);
        }
    }
}
