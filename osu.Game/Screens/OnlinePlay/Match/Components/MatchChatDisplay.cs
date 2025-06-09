// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Game.Online.Chat;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Match.Components
{
    public partial class MatchChatDisplay : StandAloneChatDisplay
    {
        [Resolved]
        private ChannelManager? channelManager { get; set; }

        private readonly Room room;
        private readonly bool leaveChannelOnDispose;

        public MatchChatDisplay(Room room, bool leaveChannelOnDispose = true)
            : base(true)
        {
            this.room = room;
            this.leaveChannelOnDispose = leaveChannelOnDispose;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            room.PropertyChanged += onRoomPropertyChanged;
            updateChannel();
        }

        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Room.ChannelId))
                updateChannel();
        }

        private void updateChannel()
        {
            if (room.RoomID == null || room.ChannelId == 0)
                return;

            Channel.Value = channelManager?.JoinChannel(new Channel { Id = room.ChannelId, Type = ChannelType.Multiplayer, Name = $"#lazermp_{room.RoomID.Value}" });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            room.PropertyChanged -= onRoomPropertyChanged;

            if (leaveChannelOnDispose)
                channelManager?.LeaveChannel(Channel.Value);
        }
    }
}
