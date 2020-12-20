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

        /// <summary>
        /// The time in milliseconds to wait between polls.
        /// Setting to zero stops all polling.
        /// </summary>
        public new readonly Bindable<double> TimeBetweenPolls = new Bindable<double>();

        public readonly Bindable<bool> InitialRoomsReceived = new Bindable<bool>();

        [Resolved]
        protected IAPIProvider API { get; private set; }

        protected RoomPollingComponent()
        {
            TimeBetweenPolls.BindValueChanged(time => base.TimeBetweenPolls = time.NewValue);
        }

        protected void NotifyRoomsReceived(List<Room> rooms)
        {
            InitialRoomsReceived.Value = true;
            RoomsReceived?.Invoke(rooms);
        }
    }
}
