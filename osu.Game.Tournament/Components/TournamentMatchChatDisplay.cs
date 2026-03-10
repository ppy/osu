// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Components
{
    public partial class TournamentMatchChatDisplay : StandAloneChatDisplay
    {
        private readonly Bindable<string> channelName = new Bindable<string>();

        private ChannelManager? manager;

        [Resolved]
        private LadderInfo ladderInfo { get; set; } = null!;

        public TournamentMatchChatDisplay()
        {
            RelativeSizeAxes = Axes.X;
            Height = 144;
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;

            CornerRadius = 0;
        }

        [BackgroundDependencyLoader]
        private void load(MatchIPCInfo ipc, IAPIProvider api)
        {
            AddInternal(manager = new ChannelManager(api));
            Channel.BindTo(manager.CurrentChannel);

            channelName.BindTo(ipc.ChatChannel);
            channelName.BindValueChanged(c =>
            {
                if (int.TryParse(c.OldValue, out int oldChannelId) && oldChannelId > 0)
                {
                    var joinedChannel = manager.JoinedChannels.SingleOrDefault(ch => ch.Id == oldChannelId);
                    if (joinedChannel != null)
                        manager.LeaveChannel(joinedChannel);
                }

                if (int.TryParse(c.NewValue, out int newChannelId) && newChannelId > 0)
                {
                    var channel = new Channel
                    {
                        Id = newChannelId,
                        Type = ChannelType.Public
                    };

                    manager.JoinChannel(channel);
                    manager.CurrentChannel.Value = channel;
                }
            }, true);
        }

        public void Expand() => this.FadeIn(300);

        public void Contract() => this.FadeOut(200);

        protected override ChatLine? CreateMessage(Message message)
        {
            if (message.Content.StartsWith("!mp", StringComparison.Ordinal))
                return null;

            return new MatchMessage(message, ladderInfo);
        }

        protected override StandAloneDrawableChannel CreateDrawableChannel(Channel channel) => new MatchChannel(channel);

        public partial class MatchChannel : StandAloneDrawableChannel
        {
            public MatchChannel(Channel channel)
                : base(channel)
            {
                ScrollbarVisible = false;
            }
        }

        protected partial class MatchMessage : StandAloneMessage
        {
            public MatchMessage(Message message, LadderInfo info)
                : base(message)
            {
                if (info.CurrentMatch.Value is TournamentMatch match)
                {
                    if (match.Team1.Value?.Players.Any(u => u.OnlineID == Message.Sender.OnlineID) == true)
                        UsernameColour = TournamentGame.COLOUR_RED;
                    else if (match.Team2.Value?.Players.Any(u => u.OnlineID == Message.Sender.OnlineID) == true)
                        UsernameColour = TournamentGame.COLOUR_BLUE;
                }
            }
        }
    }
}
