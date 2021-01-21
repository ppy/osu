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
        [Resolved(typeof(Room), nameof(Room.RoomID))]
        private Bindable<int?> roomId { get; set; }

        [Resolved(typeof(Room), nameof(Room.ChannelId))]
        private Bindable<int> channelId { get; set; }

        [Resolved(CanBeNull = true)]
        private ChannelManager channelManager { get; set; }

        public MatchChatDisplay()
            : base(true)
        {
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
    }
}
