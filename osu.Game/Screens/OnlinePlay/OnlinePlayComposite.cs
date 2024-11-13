// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay
{
    /// <summary>
    /// A <see cref="CompositeDrawable"/> that exposes bindables for <see cref="Room"/> properties.
    /// </summary>
    public partial class OnlinePlayComposite : CompositeDrawable
    {
        [Resolved(typeof(Room))]
        protected Bindable<Room.RoomPlaylistItemStats> PlaylistItemStats { get; private set; } = null!;

        [Resolved(typeof(Room))]
        protected BindableList<PlaylistItem> Playlist { get; private set; } = null!;

        [Resolved(typeof(Room))]
        protected Bindable<Room.RoomDifficultyRange> DifficultyRange { get; private set; } = null!;

        [Resolved(typeof(Room))]
        protected BindableList<APIUser> RecentParticipants { get; private set; } = null!;

        [Resolved(typeof(Room))]
        protected Bindable<int?> MaxAttempts { get; private set; } = null!;

        [Resolved(typeof(Room))]
        public Bindable<PlaylistAggregateScore> UserScore { get; private set; } = null!;

        [Resolved(typeof(Room))]
        protected Bindable<DateTimeOffset?> StartDate { get; private set; } = null!;

        [Resolved(typeof(Room))]
        protected Bindable<DateTimeOffset?> EndDate { get; private set; } = null!;

        [Resolved(typeof(Room))]
        protected Bindable<TimeSpan?> Duration { get; private set; } = null!;
    }
}
