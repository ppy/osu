// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Users;

namespace osu.Game.Screens.OnlinePlay
{
    public class OnlinePlayComposite : CompositeDrawable
    {
        [Resolved(typeof(Room))]
        protected Bindable<long?> RoomID { get; private set; }

        [Resolved(typeof(Room), nameof(Room.Name))]
        protected Bindable<string> RoomName { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<User> Host { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<RoomStatus> Status { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<GameType> Type { get; private set; }

        [Resolved(typeof(Room))]
        protected BindableList<PlaylistItem> Playlist { get; private set; }

        [Resolved(typeof(Room))]
        protected BindableList<User> RecentParticipants { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<int> ParticipantCount { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<int?> MaxParticipants { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<int?> MaxAttempts { get; private set; }

        [Resolved(typeof(Room))]
        public Bindable<PlaylistAggregateScore> UserScore { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<DateTimeOffset?> EndDate { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<RoomAvailability> Availability { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<TimeSpan?> Duration { get; private set; }

        /// <summary>
        /// The currently selected item in the <see cref="RoomSubScreen"/>.
        /// May be null if this <see cref="OnlinePlayComposite"/> is not inside a <see cref="RoomSubScreen"/>.
        /// </summary>
        [CanBeNull]
        [Resolved(CanBeNull = true)]
        protected IBindable<PlaylistItem> SelectedItem { get; private set; }
    }
}
