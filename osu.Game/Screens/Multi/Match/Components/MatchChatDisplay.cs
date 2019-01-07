// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
