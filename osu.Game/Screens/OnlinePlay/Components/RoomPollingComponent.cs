// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public abstract class RoomPollingComponent : PollingComponent
    {
        /// <summary>
        /// Invoked when any <see cref="Room"/>s have been received from the API.
        /// <para>
        /// Any <see cref="Room"/>s present locally but not returned by this event are to be removed from display.
        /// If null, the display of local rooms is reset to an initial state.
        /// </para>
        /// </summary>
        public Action<List<Room>> RoomsReceived;

        [Resolved]
        protected IAPIProvider API { get; private set; }

        protected void NotifyRoomsReceived(List<Room> rooms) => RoomsReceived?.Invoke(rooms);
    }
}
