// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public class TournamentMatchChatDisplay : StandAloneChatDisplay
    {
        private readonly Bindable<string> chatChannel = new Bindable<string>();

        private ChannelManager manager;

        public TournamentMatchChatDisplay()
        {
            RelativeSizeAxes = Axes.X;
            Y = 100;
            Size = new Vector2(0.45f, 112);
            Margin = new MarginPadding(10);
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
        }

        [BackgroundDependencyLoader(true)]
        private void load(MatchIPCInfo ipc)
        {
            if (ipc != null)
            {
                chatChannel.BindTo(ipc.ChatChannel);
                chatChannel.BindValueChanged(c =>
                {
                    if (string.IsNullOrWhiteSpace(c.NewValue))
                        return;

                    int id = int.Parse(c.NewValue);

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

        protected override ChatLine CreateMessage(Message message) => new MatchMessage(message);

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
