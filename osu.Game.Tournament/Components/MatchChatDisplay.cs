// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat;
using osu.Game.Tournament.IPC;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public class MatchChatDisplay : StandAloneChatDisplay
    {
        private readonly Bindable<string> chatChannel = new Bindable<string>();

        protected override ChatLine CreateMessage(Message message) => new MatchMessage(message);

        private ChannelManager manager;

        [BackgroundDependencyLoader(true)]
        private void load(MatchIPCInfo ipc)
        {
            if (ipc != null)
            {
                chatChannel.BindTo(ipc.ChatChannel);
                chatChannel.BindValueChanged(channelString =>
                {
                    if (string.IsNullOrWhiteSpace(channelString))
                        return;

                    int id = int.Parse(channelString);

                    if (id <= 0) return;

                    if (manager == null)
                    {
                        AddInternal(manager = new ChannelManager());
                        Channel.BindTo(manager.CurrentChannel);
                    }

                    foreach (var ch in manager.JoinedChannels.ToList())
                        manager.LeaveChannel(ch);

                    var channel = new Channel
                    {
                        Id = id,
                        Type = ChannelType.Public
                    };

                    manager.JoinChannel(channel);
                    manager.CurrentChannel.Value = channel;
                }, true);
            }
        }

        protected class MatchMessage : StandAloneMessage
        {
            public MatchMessage(Message message)
                : base(message)
            {
            }

            [BackgroundDependencyLoader]
            private void load(LadderInfo info)
            {
                //if (info.CurrentMatch.Value.Team1.Value.Players.Any(u => u.Id == Message.Sender.Id))
                //    ColourBox.Colour = red;
                //else if (info.CurrentMatch.Value.Team2.Value.Players.Any(u => u.Id == Message.Sender.Id))
                //    ColourBox.Colour = blue;
                //else if (Message.Sender.Colour != null)
                //    SenderText.Colour = ColourBox.Colour = OsuColour.FromHex(Message.Sender.Colour);
            }

            private readonly Color4 red = new Color4(186, 0, 18, 255);
            private readonly Color4 blue = new Color4(17, 136, 170, 255);
        }
    }
}
