// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Participants
{
    public partial class ParticipantsList : VirtualisedListContainer<MultiplayerRoomUser, ParticipantPanel>
    {
        private BindableList<MultiplayerRoomUser> participants => RowData;
        private MultiplayerRoomUser? currentHost;
        private ParticipantsSortMode? currentSortMode;
        private SortDirection? currentSortDirection;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        public ParticipantsList()
            : base(ParticipantPanel.HEIGHT, initialPoolSize: 20)
        {
        }

        protected override ScrollContainer<Drawable> CreateScrollContainer() => new OsuScrollContainer
        {
            ScrollbarVisible = false,
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.RoomUpdated += onRoomUpdated;
            updateState();
        }

        private void onRoomUpdated() => Scheduler.AddOnce(updateState);
        private void updateState()
        {
            if (client.Room == null)
                participants.Clear();
            else
            {
                // Remove panels for users no longer in the room.
                for (int i = participants.Count - 1; i >= 0; i--)
                {
                    var participant = participants[i];

                    // Note that we *must* use reference equality here, as this call is scheduled and a user may have left and joined since it was last run.
                    if (client.Room.Users.All(u => !ReferenceEquals(participant, u)))
                        participants.RemoveAt(i);
                }

                // Add panels for all users new to the room.
                foreach (var user in client.Room.Users.Except(participants))
                    participants.Add(user);
                    if (currentHost == null || !ReferenceEquals(currentHost, client.Room.Host))
                {
                    currentHost = null;

                    // Change position of new host to display above all participants.
                    if (client.Room.Host != null)
                    {
                        currentHost = participants.SingleOrDefault(u => ReferenceEquals(u, client.Room.Host));
                        int currentHostIndex = participants.IndexOf(client.Room.Host);

                        if (currentHostIndex > 0)
                        {
                            participants.Move(currentHostIndex, 0);
                            currentHost = participants[0];
                        }
                    }
                }

                UpdateParticipants();
            }
        }
        /// <summary>
        /// Reorders existing panels based on sort criteria.
        /// </summary>
        /// <param name="sortMode">Optional sort mode to apply and store for future updates</param>
        /// <param name="sortDirection">Optional sort direction to apply and store for future updates</param>
        public void UpdateParticipants(ParticipantsSortMode? sortMode = null, SortDirection? sortDirection = null)
        {
            if (client.Room == null)
                return;

            // Update stored sort settings if provided
            if (sortMode.HasValue)
                currentSortMode = sortMode.Value;
            if (sortDirection.HasValue)
                currentSortDirection = sortDirection.Value;

            // Only reorder if we have sort settings
            if (!currentSortMode.HasValue || !currentSortDirection.HasValue)
                return;

            IList<MultiplayerRoomUser> sortedUsers = client.Room.Users.ToList();

            switch (currentSortMode.Value)
            {
                case ParticipantsSortMode.Alphabetical:
                    sortedUsers = currentSortDirection.Value == SortDirection.Ascending
                        ? sortedUsers.OrderBy(u => u.User!.Username).ToList()
                        : sortedUsers.OrderByDescending(u => u.User!.Username).ToList();
                    break;

                case ParticipantsSortMode.Country:
                    sortedUsers = sortByCountryWithRankSecondary(sortedUsers);
                    break;

                case ParticipantsSortMode.Rank:
                    sortedUsers = currentSortDirection.Value == SortDirection.Ascending
                        ? sortedUsers.OrderBy(u => u.User!.Statistics?.GlobalRank ?? int.MaxValue).ToList()
                        : sortedUsers.OrderByDescending(u => u.User!.Statistics?.GlobalRank ?? int.MaxValue).ToList();
                    break;
            }
            // Reorder existing participants to match the sorted user list
            for (int i = 0; i < sortedUsers.Count; i++)
            {
                var user = sortedUsers[i];
                var existingIndex = participants.IndexOf(user);

                if (existingIndex != -1 && existingIndex != i)
                    participants.Move(existingIndex, i);
            }

            // Ensure host is still positioned at the top (override sort for host)
            if (client.Room?.Host != null)
            {
                var hostIndex = participants.IndexOf(client.Room.Host);
                if (hostIndex > 0)
                {
                    participants.Move(hostIndex, 0);
                    currentHost = participants[0];
                }
            }
        }
        /// <summary>
        /// Sorts users by country first, then by rank within each country (best rank first).
        /// </summary>
        private IList<MultiplayerRoomUser> sortByCountryWithRankSecondary(IList<MultiplayerRoomUser> users)
        {
            if (currentSortDirection!.Value == SortDirection.Ascending)
            {
                return users
                    .GroupBy(u => u.User!.CountryCode.ToString())
                    .OrderBy(g => g.Key)
                    .SelectMany(g => g.OrderBy(u => u.User!.Statistics?.GlobalRank ?? int.MaxValue))
                    .ToList();
            }
            else
            {
                return users
                    .GroupBy(u => u.User!.CountryCode.ToString())
                    .OrderByDescending(g => g.Key)
                    .SelectMany(g => g.OrderBy(u => u.User!.Statistics?.GlobalRank ?? int.MaxValue))
                    .ToList();
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
                client.RoomUpdated -= onRoomUpdated;
        }
    }
}
