// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.Chat;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Match.Components
{
    public class MatchChatDisplay : StandAloneChatDisplay
    {
        private readonly IBindable<long?> roomId = new Bindable<long?>();
        private readonly IBindable<int> channelId = new Bindable<int>();

        [Resolved(CanBeNull = true)]
        private ChannelManager channelManager { get; set; }

        private readonly bool leaveChannelOnDispose;

        public MatchChatDisplay(Room room, bool leaveChannelOnDispose = true)
            : base(true)
        {
            this.leaveChannelOnDispose = leaveChannelOnDispose;

            roomId.BindTo(room.RoomID);
            channelId.BindTo(room.ChannelId);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            channelId.BindValueChanged(_ => updateChannel(), true);
        }

        private void updateChannel()
        {
            if (roomId.Value == null || channelId.Value == 0)
                return;

            Channel.Value = channelManager?.JoinChannel(new Channel { Id = channelId.Value, Type = ChannelType.Multiplayer, Name = $"#lazermp_{roomId.Value}" });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (leaveChannelOnDispose)
                channelManager?.LeaveChannel(Channel.Value);
        }
    }
}
