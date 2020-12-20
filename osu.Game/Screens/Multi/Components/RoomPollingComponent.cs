// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.Multi.Components
{
    public abstract class RoomPollingComponent : PollingComponent
    {
        public Action<List<Room>> RoomsReceived;

        public readonly Bindable<bool> InitialRoomsReceived = new Bindable<bool>();

        [Resolved]
        protected IAPIProvider API { get; private set; }

        protected void NotifyRoomsReceived(List<Room> rooms)
        {
            InitialRoomsReceived.Value = true;
            RoomsReceived?.Invoke(rooms);
        }
    }
}
