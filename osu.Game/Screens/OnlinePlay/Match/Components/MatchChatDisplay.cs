// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.Chat;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Match.Components
{
    public partial class MatchChatDisplay : StandAloneChatDisplay
    {
        private readonly IBindable<int> channelId = new Bindable<int>();

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

            // Required for the time being since this component is created prior to the room being joined.
            channelId.BindTo(room.ChannelId);
            channelId.BindValueChanged(_ => updateChannel(), true);
        }

        private void updateChannel()
        {
            if (room.RoomID.Value == null || channelId.Value == 0)
                return;

            Channel.Value = channelManager?.JoinChannel(new Channel { Id = channelId.Value, Type = ChannelType.Multiplayer, Name = $"#lazermp_{room.RoomID.Value}" });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (leaveChannelOnDispose)
                channelManager?.LeaveChannel(Channel.Value);
        }
    }
}
