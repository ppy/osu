// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Online.Chat;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class MatchChatDisplay : StandAloneChatDisplay
    {
        private readonly Room room;

        [Resolved(CanBeNull = true)]
        private ChannelManager channelManager { get; set; }

        public MatchChatDisplay(Room room)
            : base(true)
        {
            this.room = room;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            room.RoomID.BindValueChanged(v => updateChannel(), true);
        }

        private void updateChannel()
        {
            if (room.RoomID.Value != null)
                Channel.Value = channelManager?.JoinChannel(new Channel { Id = room.ChannelId, Type = ChannelType.Multiplayer, Name = $"#mp_{room.RoomID}" });
        }
    }
}
