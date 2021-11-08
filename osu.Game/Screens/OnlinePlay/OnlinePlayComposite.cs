// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Match;

namespace osu.Game.Screens.OnlinePlay
{
    /// <summary>
    /// A <see cref="CompositeDrawable"/> that exposes bindables for <see cref="Room"/> properties.
    /// </summary>
    public class OnlinePlayComposite : CompositeDrawable
    {
        [Resolved(typeof(Room))]
        protected Bindable<long?> RoomID { get; private set; }

        [Resolved(typeof(Room), nameof(Room.Name))]
        protected Bindable<string> RoomName { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<APIUser> Host { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<RoomStatus> Status { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<MatchType> Type { get; private set; }

        [Resolved(typeof(Room))]
        protected BindableList<PlaylistItem> Playlist { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<RoomCategory> Category { get; private set; }

        [Resolved(typeof(Room))]
        protected BindableList<APIUser> RecentParticipants { get; private set; }

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

        [Resolved(typeof(Room), nameof(Room.Password))]
        public Bindable<string> Password { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<TimeSpan?> Duration { get; private set; }

        [Resolved(CanBeNull = true)]
        private IBindable<PlaylistItem> subScreenSelectedItem { get; set; }

        /// <summary>
        /// The currently selected item in the <see cref="RoomSubScreen"/>, or the last item from <see cref="Playlist"/>
        /// if this <see cref="OnlinePlayComposite"/> is not within a <see cref="RoomSubScreen"/>.
        /// </summary>
        protected readonly Bindable<PlaylistItem> SelectedItem = new Bindable<PlaylistItem>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            subScreenSelectedItem?.BindValueChanged(_ => UpdateSelectedItem());
            Playlist.BindCollectionChanged((_, __) => UpdateSelectedItem(), true);
        }

        protected virtual void UpdateSelectedItem()
            => SelectedItem.Value = RoomID.Value == null || subScreenSelectedItem == null
                ? Playlist.LastOrDefault()
                : subScreenSelectedItem.Value;
    }
}
