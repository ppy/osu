// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Lounge.Components;

namespace osu.Game.Tests.Visual.OnlinePlay
{
    /// <summary>
    /// Interface that defines the dependencies required for online play test scenes.
    /// </summary>
    public interface IOnlinePlayTestDependencies
    {
        /// <summary>
        /// The cached <see cref="Room"/>.
        /// </summary>
        Bindable<Room> SelectedRoom { get; }

        /// <summary>
        /// The cached <see cref="IRoomManager"/>
        /// </summary>
        IRoomManager RoomManager { get; }

        /// <summary>
        /// The cached <see cref="FilterCriteria"/>.
        /// </summary>
        Bindable<FilterCriteria> Filter { get; }

        /// <summary>
        /// The cached <see cref="OngoingOperationTracker"/>.
        /// </summary>
        OngoingOperationTracker OngoingOperationTracker { get; }

        /// <summary>
        /// The cached <see cref="OnlinePlayBeatmapAvailabilityTracker"/>.
        /// </summary>
        OnlinePlayBeatmapAvailabilityTracker AvailabilityTracker { get; }
    }
}
