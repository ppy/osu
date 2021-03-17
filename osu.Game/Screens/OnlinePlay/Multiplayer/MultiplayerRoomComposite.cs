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
        protected StatefulMultiplayerClient Client { get; private set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Client.RoomUpdated += OnRoomUpdated;
            OnRoomUpdated();
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
                Client.RoomUpdated -= OnRoomUpdated;

            base.Dispose(isDisposing);
        }
    }
}
