// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Database;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay;

namespace osu.Game.Tests.Visual.OnlinePlay
{
    /// <summary>
    /// Interface that defines the dependencies required for online play test scenes.
    /// </summary>
    public interface IOnlinePlayTestSceneDependencies
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
        /// The cached <see cref="OngoingOperationTracker"/>.
        /// </summary>
        OngoingOperationTracker OngoingOperationTracker { get; }

        /// <summary>
        /// The cached <see cref="OnlinePlayBeatmapAvailabilityTracker"/>.
        /// </summary>
        OnlinePlayBeatmapAvailabilityTracker AvailabilityTracker { get; }

        /// <summary>
        /// The cached <see cref="UserLookupCache"/>.
        /// </summary>
        TestUserLookupCache UserLookupCache { get; }

        /// <summary>
        /// The cached <see cref="BeatmapLookupCache"/>.
        /// </summary>
        BeatmapLookupCache BeatmapLookupCache { get; }
    }
}
