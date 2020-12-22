// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Game.Screens.Multi.Components;

namespace osu.Game.Screens.Multi.Timeshift
{
    public class TimeshiftRoomManager : RoomManager
    {
        public readonly Bindable<double> TimeBetweenListingPolls = new Bindable<double>();
        public readonly Bindable<double> TimeBetweenSelectionPolls = new Bindable<double>();

        protected override IEnumerable<RoomPollingComponent> CreatePollingComponents() => new RoomPollingComponent[]
        {
            new ListingPollingComponent { TimeBetweenPolls = { BindTarget = TimeBetweenListingPolls } },
            new SelectionPollingComponent { TimeBetweenPolls = { BindTarget = TimeBetweenSelectionPolls } }
        };
    }
}
