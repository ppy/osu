// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Participants
{
    public partial class ParticipantsList : VirtualisedListContainer<MultiplayerRoomUser, ParticipantPanel>
    {
        private BindableList<MultiplayerRoomUser> participants => RowData;

        private MultiplayerRoomUser? currentHost;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

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

                if (currentHost == null || !currentHost.Equals(client.Room.Host))
                {
                    currentHost = null;

                    // Change position of new host to display above all participants.
                    if (client.Room.Host != null)
                    {
                        currentHost = participants.SingleOrDefault(u => u.Equals(client.Room.Host));
                        int currentHostIndex = participants.IndexOf(client.Room.Host);

                        if (currentHostIndex > 0)
                        {
                            participants.Move(currentHostIndex, 0);
                            currentHost = participants[0];
                        }
                    }
                }
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
