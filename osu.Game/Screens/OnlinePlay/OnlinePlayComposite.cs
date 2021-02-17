// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Specialized;
using System.Linq;
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
        /// The currently selected item in the <see cref="RoomSubScreen"/>, or the first item from <see cref="Playlist"/>
        /// if this <see cref="OnlinePlayComposite"/> is not within a <see cref="RoomSubScreen"/>.
        /// </summary>
        protected IBindable<PlaylistItem> SelectedItem => selectedItem;

        private readonly Bindable<PlaylistItem> selectedItem = new Bindable<PlaylistItem>();

        [CanBeNull]
        [Resolved(CanBeNull = true)]
        private IBindable<PlaylistItem> subScreenSelectedItem { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (subScreenSelectedItem != null)
                subScreenSelectedItem.BindValueChanged(onSelectedItemChanged, true);
            else
                Playlist.BindCollectionChanged(onPlaylistChanged, true);
        }

        /// <summary>
        /// Invoked when the selected item from within a <see cref="RoomSubScreen"/> changes.
        /// Does not occur when this <see cref="OnlinePlayComposite"/> is outside a <see cref="RoomSubScreen"/>.
        /// </summary>
        private void onSelectedItemChanged(ValueChangedEvent<PlaylistItem> item)
        {
            // If the room hasn't been created yet, fall-back to the first item from the playlist.
            selectedItem.Value = RoomID.Value == null ? Playlist.FirstOrDefault() : item.NewValue;
        }

        /// <summary>
        /// Invoked when the playlist changes.
        /// Does not occur when this <see cref="OnlinePlayComposite"/> is inside a <see cref="RoomSubScreen"/>.
        /// </summary>
        private void onPlaylistChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            selectedItem.Value = Playlist.FirstOrDefault();
        }
    }
}
