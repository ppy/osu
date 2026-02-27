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
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Rulesets;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Participants
{
    public partial class ParticipantsList : VirtualisedListContainer<MultiplayerRoomUser, ParticipantPanel>
    {
        private BindableList<MultiplayerRoomUser> participants => RowData;
        private MultiplayerRoomUser? currentHost;
        public readonly Bindable<ParticipantsSortMode> SortMode = new Bindable<ParticipantsSortMode>();
        public readonly Bindable<SortDirection> SortDirection = new Bindable<SortDirection>();

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private IRulesetStore rulesets { get; set; } = null!;

        public ParticipantsList()
            : base(ParticipantPanel.HEIGHT + 1, initialPoolSize: 20)
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

            SortMode.BindValueChanged(_ => updateParticipants());
            SortDirection.BindValueChanged(_ => updateParticipants());

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

                updateParticipants();
            }
        }

        /// <summary>
        /// Reorders existing panels based on sort criteria.
        /// </summary>
        private void updateParticipants()
        {
            if (client.Room == null)
                return;

            IList<MultiplayerRoomUser> sortedUsers = client.Room.Users.ToList();

            switch (SortMode.Value)
            {
                case ParticipantsSortMode.Alphabetical:
                    sortedUsers = SortDirection.Value == Overlays.SortDirection.Ascending
                        ? sortedUsers.OrderBy(u => u.User!.Username).ToList()
                        : sortedUsers.OrderByDescending(u => u.User!.Username).ToList();
                    break;

                case ParticipantsSortMode.Country:
                    sortedUsers = sortByCountryWithRankSecondary(sortedUsers);
                    break;

                case ParticipantsSortMode.Rank:
                    sortedUsers = sortByRankForCurrentRuleset(sortedUsers);
                    break;
            }

            // Reorder existing participants to match the sorted user list
            for (int i = 0; i < sortedUsers.Count; i++)
            {
                var user = sortedUsers[i];
                int existingIndex = participants.IndexOf(user);

                if (existingIndex != -1 && existingIndex != i)
                    participants.Move(existingIndex, i);
            }

            // Ensure host is still positioned at the top (override sort for host)
            if (client.Room?.Host != null)
            {
                int hostIndex = participants.IndexOf(client.Room.Host);

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
            if (SortDirection.Value == Overlays.SortDirection.Ascending)
            {
                return users
                .GroupBy(u => u.User!.CountryCode.ToString())
                .OrderBy(g => g.Key)
                .SelectMany(g => g.OrderBy(getCurrentRulesetRank))
                .ToList();
            }
            else
            {
                return users
                .GroupBy(u => u.User!.CountryCode.ToString())
                .OrderByDescending(g => g.Key)
                .SelectMany(g => g.OrderBy(getCurrentRulesetRank))
                .ToList();
            }
        }

        /// <summary>
        /// Sorts users by their rank in the current ruleset.
        /// </summary>
        private IList<MultiplayerRoomUser> sortByRankForCurrentRuleset(IList<MultiplayerRoomUser> users)
        {
            if (SortDirection.Value == Overlays.SortDirection.Ascending)
            {
                return users.OrderBy(getCurrentRulesetRank).ToList();
            }
            else
            {
                return users.OrderByDescending(getCurrentRulesetRank).ToList();
            }
        }

        /// <summary>
        /// Gets the current ruleset rank for a user, similar to how ParticipantPanel does it.
        /// </summary>
        private int getCurrentRulesetRank(MultiplayerRoomUser user)
        {
            if (client.Room?.GetCurrentItem() is not MultiplayerPlaylistItem currentItem)
                return int.MaxValue;

            int userRulesetId = user.RulesetId ?? currentItem.RulesetID;
            Ruleset? userRuleset = rulesets.GetRuleset(userRulesetId)?.CreateInstance();

            if (userRuleset == null)
                return int.MaxValue;

            int? currentModeRank = user.User?.RulesetsStatistics?.GetValueOrDefault(userRuleset.ShortName)?.GlobalRank;
            return currentModeRank ?? int.MaxValue;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
                client.RoomUpdated -= onRoomUpdated;
        }
    }
}
