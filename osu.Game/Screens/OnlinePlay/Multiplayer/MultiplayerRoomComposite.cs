// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public abstract class MultiplayerRoomComposite : OnlinePlayComposite
    {
        [CanBeNull]
        protected MultiplayerRoom Room => Client.Room;

        [Resolved]
        protected MultiplayerClient Client { get; private set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Client.RoomUpdated += invokeOnRoomUpdated;
            Client.UserLeft += invokeUserLeft;
            Client.UserKicked += invokeUserKicked;
            Client.UserJoined += invokeUserJoined;

            OnRoomUpdated();
        }

        private void invokeOnRoomUpdated() => Scheduler.AddOnce(OnRoomUpdated);
        private void invokeUserJoined(MultiplayerRoomUser user) => Scheduler.AddOnce(UserJoined, user);
        private void invokeUserKicked(MultiplayerRoomUser user) => Scheduler.AddOnce(UserKicked, user);
        private void invokeUserLeft(MultiplayerRoomUser user) => Scheduler.AddOnce(UserLeft, user);

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
        /// Invoked when any change occurs to the multiplayer room.
        /// </summary>
        protected virtual void OnRoomUpdated()
        {
        }

        protected override void Dispose(bool isDisposing)
        {
            if (Client != null)
            {
                Client.RoomUpdated -= invokeOnRoomUpdated;
                Client.UserLeft -= invokeUserLeft;
                Client.UserKicked -= invokeUserKicked;
                Client.UserJoined -= invokeUserJoined;
            }

            base.Dispose(isDisposing);
        }
    }
}
