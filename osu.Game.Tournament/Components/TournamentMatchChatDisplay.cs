// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Components
{
    public partial class TournamentMatchChatDisplay : StandAloneChatDisplay
    {
        private readonly Bindable<string> chatChannel = new Bindable<string>();

        private ChannelManager? manager;

        [Resolved]
        private LadderInfo ladderInfo { get; set; } = null!;

        public TournamentMatchChatDisplay(float cornerRadius = 0, bool AutoSizeY = false, bool RelativeSizeY = false, float bkgAlpha = 0.7f)
        {
            AutoSizeAxes = AutoSizeY ? Axes.Y : Axes.None;
            RelativeSizeAxes = RelativeSizeY ? Axes.Both : Axes.X;
            if (!AutoSizeY) Height = RelativeSizeY ? 1f : 144;
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;

            Background.Alpha = bkgAlpha;

            CornerRadius = cornerRadius;
        }

        [BackgroundDependencyLoader]
        private void load(MatchIPCInfo? ipc, IAPIProvider api)
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
                        AddInternal(manager = new ChannelManager(api));
                        Channel.BindTo(manager.CurrentChannel);
                    }

                    Channel? channel = manager.JoinedChannels.FirstOrDefault(p => p.Id == id);

                    if (channel == null)
                    {
                        // channel = new Channel(new APIUser { Id = 3 })
                        channel = new Channel
                        {
                            Id = id,
                            Type = ChannelType.Public,
                            // Type = ChannelType.PM,
                        };
                        manager.JoinChannel(channel);
                    }

                    manager.CurrentChannel.Value = channel;
                }, true);
            }
        }

        public void ChangeRadius(int radius)
        {
            CornerRadius = radius;
        }

        public void Expand() => this.FadeIn(300);

        public void Contract() => this.FadeOut(200);

        protected override ChatLine CreateMessage(Message message)
        {
            var currentMatch = ladderInfo.CurrentMatch;
            bool isCommand = false;

            // Try to recognize and verify bot commmands
            if (currentMatch.Value != null && currentMatch.Value.Round.Value != null)
            {
                isCommand = message.Content[0] == '[' && message.Content[1] == '*' && message.Content[2] == ']';
                bool isRef = currentMatch.Value.Round.Value.RefereeId.Value != null
                    && currentMatch.Value.Round.Value.RefereeId.Value != 0
                    && message.SenderId == currentMatch.Value.Round.Value.RefereeId.Value;
                // Automatically block duplicate messages, since we have multiple chat displays available.
                if ((isRef || currentMatch.Value.Round.Value.TrustAll.Value)
                    && isCommand && !currentMatch.Value.PendingMsgs.Any(p => p.Equals(message)))
                {
                    currentMatch.Value.PendingMsgs.Add(message);
                }
                if (!currentMatch.Value.ChatMsgs.Any(p => p.Equals(message)))
                {
                    currentMatch.Value.ChatMsgs.Add(message);
                }
            }

            return new MatchMessage(message, ladderInfo, isCommand);
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
            public MatchMessage(Message message, LadderInfo info, bool isCommand)
                : base(message)
            {
                // Disable line background alternating, see https://github.com/ppy/osu/pull/29137
                AlternatingBackground = false;
                IsStrong = isCommand;

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
