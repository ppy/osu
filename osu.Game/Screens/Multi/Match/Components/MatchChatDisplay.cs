// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Game.Online.Chat;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.Multi.Match.Components
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

            roomId.BindValueChanged(v => updateChannel(), true);
        }

        private void updateChannel()
        {
            if (roomId.Value != null)
                Channel.Value = channelManager?.JoinChannel(new Channel { Id = channelId, Type = ChannelType.Multiplayer, Name = $"#mp_{roomId.Value}" });
        }
    }
}
