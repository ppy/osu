// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
    public partial class ParticipantsList : VirtualisedListContainer<Slot, ParticipantPanel>
    {
        private BindableList<Slot> slots => RowData;

        private Slot? currentHost;

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
            {
                slots.Clear();
                return;
            }

            // pathway for handling rooms with participant count limit and slots
            if (client.Room.MatchState is StandardMatchRoomState standardMatchRoomState && standardMatchRoomState.Slots is int?[] slotUserIds)
            {
                // reset host tracking - in slots mode the host's position is decided solely by their slot
                // the reset has the side benefit of getting the host pinned to top of list again if slots are turned off (see logic lower down).
                currentHost = null;

                if (slots.Count > slotUserIds.Length)
                    slots.RemoveRange(slotUserIds.Length, slots.Count - slotUserIds.Length);

                for (byte i = 0; i < slotUserIds.Length; ++i)
                {
                    var participant = slotUserIds[i] == null ? Slot.Empty(i) : Slot.FromUser(client.Room.Users.Single(u => u.UserID == slotUserIds[i]));

                    if (i >= slots.Count)
                        slots.Add(participant);
                    if (!participant.Equals(slots[i]))
                        slots[i] = participant;
                }

                return;
            }

            // Remove panels for empty slots & users no longer in the room.
            for (int i = slots.Count - 1; i >= 0; i--)
            {
                var slot = slots[i];

                // Note that we *must* use reference equality here, as this call is scheduled and a user may have left and joined since it was last run.
                if (slot.IsEmpty || client.Room.Users.All(u => !ReferenceEquals(slot.User, u)))
                    slots.RemoveAt(i);
            }

            // This assertion guarantees that all subsequent accesses to `User` of any `Slot` is safe.
            // Unfortunately static analysis is not smart enough to pick this up, so there'll be a lot of `.AsNonNull()` lower down.
            Debug.Assert(slots.All(p => !p.IsEmpty));

            // Add panels for all users new to the room.
            foreach (var user in client.Room.Users.Except(slots.Select(u => u.User.AsNonNull())))
                slots.Add(Slot.FromUser(user));

            if (currentHost == null || !currentHost.User.AsNonNull().Equals(client.Room.Host))
            {
                currentHost = null;

                // Change position of new host to display above all participants.
                if (client.Room.Host != null)
                {
                    currentHost = slots.SingleOrDefault(u => u.User.AsNonNull().Equals(client.Room.Host));
                    int currentHostIndex = currentHost == null ? -1 : slots.IndexOf(currentHost);

                    if (currentHostIndex > 0)
                    {
                        slots.Move(currentHostIndex, 0);
                        currentHost = slots[0];
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

    public record Slot
    {
        [MemberNotNullWhen(false, nameof(User))]
        [MemberNotNullWhen(true, nameof(SlotId))]
        public bool IsEmpty { get; }

        public MultiplayerRoomUser? User { get; }

        public byte? SlotId { get; }

        private Slot(bool isEmpty, MultiplayerRoomUser? user, byte? slotId)
        {
            IsEmpty = isEmpty;
            User = user;
            SlotId = slotId;
        }

        public static Slot FromUser(MultiplayerRoomUser user) => new Slot(false, user, null);

        public static Slot Empty(byte slotId) => new Slot(true, null, slotId);
    }
}
