// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osu.Game.Tournament.IPC;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public class MatchChatDisplay : CompositeDrawable
    {
        private Channel lastChannel;
        public readonly Bindable<Channel> Channel = new Bindable<Channel>();
        private readonly FillFlowContainer messagesFlow;

        public MatchChatDisplay()
        {
            CornerRadius = 10;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black,
                    Alpha = 0.8f,
                    RelativeSizeAxes = Axes.Both,
                },
                messagesFlow = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    LayoutEasing = Easing.Out,
                    LayoutDuration = 500,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Direction = FillDirection.Vertical,
                },
            };

            Channel.BindValueChanged(channelChanged);
        }

        private readonly Bindable<string> chatChannel = new Bindable<string>();

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

        private void channelChanged(Channel channel)
        {
            if (lastChannel != null)
                lastChannel.NewMessagesArrived -= newMessages;

            lastChannel = channel;
            messagesFlow.Clear();

            if (channel == null) return;

            channel.NewMessagesArrived += newMessages;
        }

        private void newMessages(IEnumerable<Message> messages)
        {
            var excessChildren = messagesFlow.Children.Count - 10;
            if (excessChildren > 0)
            {
                foreach (var c in messagesFlow.Children.Take(excessChildren))
                    c.Expire();
            }

            foreach (var message in messages)
            {
                var formatted = MessageFormatter.FormatMessage(message);
                messagesFlow.Add(new MatchMessage(formatted) { Y = messagesFlow.Height });
            }
        }

        private class MatchMessage : CompositeDrawable
        {
            private readonly Message message;

            public MatchMessage(Message message)
            {
                this.message = message;
            }

            private readonly Color4 red = new Color4(186, 0, 18, 255);
            private readonly Color4 blue = new Color4(17, 136, 170, 255);

            [BackgroundDependencyLoader]
            private void load(LadderInfo info)
            {
                Circle colourBox;

                Margin = new MarginPadding(3);

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                OsuSpriteText senderText;
                InternalChildren = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 0.2f,
                                Children = new Drawable[]
                                {
                                    senderText = new OsuSpriteText
                                    {
                                        Font = @"Exo2.0-Bold",
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        Text = message.Sender.ToString()
                                    }
                                }
                            },
                            new Container
                            {
                                Size = new Vector2(8, OsuSpriteText.FONT_SIZE),
                                Margin = new MarginPadding { Horizontal = 3 },
                                Children = new Drawable[]
                                {
                                    colourBox = new Circle
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Size = new Vector2(8),
                                    },
                                }
                            },
                            new OsuTextFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Width = 0.5f,
                                Text = message.DisplayContent
                            }
                        }
                    },
                };

                if (message.Sender.Colour != null)
                {
                    senderText.Colour = colourBox.Colour = OsuColour.FromHex(message.Sender.Colour);
                }
                else if (info.CurrentMatch.Value.Team1.Value.Players.Any(u => u.Id == message.Sender.Id))
                {
                    senderText.Colour = colourBox.Colour = red;
                }
                else if (info.CurrentMatch.Value.Team2.Value.Players.Any(u => u.Id == message.Sender.Id))
                {
                    senderText.Colour = colourBox.Colour = blue;
                }
            }
        }

        public void Contract()
        {
            this.FadeIn(300);
            this.MoveToY(0, 500, Easing.OutQuint);
        }

        public void Expand()
        {
            this.FadeOut(200);
            this.MoveToY(100, 500, Easing.In);
        }
    }
}
