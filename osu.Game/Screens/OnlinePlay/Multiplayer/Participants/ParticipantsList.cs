// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Participants
{
    public partial class ParticipantsList : CompositeDrawable
    {
        private FillFlowContainer<ParticipantPanel> panels = null!;
        private ParticipantPanel? currentHostPanel;
        private ParticipantsSortMode? currentSortMode;
        private SortDirection? currentSortDirection;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new OsuScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                ScrollbarVisible = false,
                Child = panels = new FillFlowContainer<ParticipantPanel>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 2)
                }
            };
        }

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
                panels.Clear();
            else
            {
                // Remove panels for users no longer in the room.
                foreach (var p in panels)
                {
                    // Note that we *must* use reference equality here, as this call is scheduled and a user may have left and joined since it was last run.
                    if (client.Room.Users.All(u => !ReferenceEquals(p.User, u)))
                        p.Expire();
                }

                // Add panels for all users new to the room.
                foreach (var user in client.Room.Users.Except(panels.Select(p => p.User)))
                    panels.Add(new ParticipantPanel(user));

                if (currentHostPanel == null || !currentHostPanel.User.Equals(client.Room.Host))
                {
                    // Reset position of previous host back to normal, if one existing.
                    if (currentHostPanel != null && panels.Contains(currentHostPanel))
                        panels.SetLayoutPosition(currentHostPanel, 0);

                    currentHostPanel = null;

                    // Change position of new host to display above all participants.
                    if (client.Room.Host != null)
                    {
                        currentHostPanel = panels.SingleOrDefault(u => u.User.Equals(client.Room.Host));

                        if (currentHostPanel != null)
                            panels.SetLayoutPosition(currentHostPanel, -1);
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

            // Reorder existing panels to match the sorted user list
            for (int i = 0; i < sortedUsers.Count; i++)
            {
                var user = sortedUsers[i];
                var panel = panels.FirstOrDefault(p => p.User.Equals(user));

                if (panel != null)
                    panels.SetLayoutPosition(panel, i);
            }

            // Ensure host is still positioned at the top (override sort for host)
            if (client.Room?.Host != null)
            {
                var hostPanel = panels.FirstOrDefault(p => p.User.Equals(client.Room.Host));
                if (hostPanel != null)
                {
                    currentHostPanel = hostPanel;
                    panels.SetLayoutPosition(hostPanel, -1);
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
