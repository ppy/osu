// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Online.Chat
{
    /// <summary>
    /// Display a chat channel in an insolated region.
    /// </summary>
    public class StandAloneChatDisplay : CompositeDrawable
    {
        public readonly Bindable<Channel> Channel = new Bindable<Channel>();

        private readonly FillFlowContainer messagesFlow;

        private Channel lastChannel;

        private readonly FocusedTextBox textbox;

        protected ChannelManager ChannelManager;

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        /// <param name="postingTextbox">Whether a textbox for posting new messages should be displayed.</param>
        public StandAloneChatDisplay(bool postingTextbox = false)
        {
            CornerRadius = 10;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black,
                    Alpha = 0.8f,
                    RelativeSizeAxes = Axes.Both
                },
                messagesFlow = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    LayoutEasing = Easing.Out,
                    LayoutDuration = 500,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Direction = FillDirection.Vertical
                }
            };

            const float textbox_height = 30;

            if (postingTextbox)
            {
                messagesFlow.Y -= textbox_height;
                AddInternal(textbox = new FocusedTextBox
                {
                    RelativeSizeAxes = Axes.X,
                    Height = textbox_height,
                    PlaceholderText = "type your message",
                    OnCommit = postMessage,
                    ReleaseFocusOnCommit = false,
                    HoldFocus = true,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                });
            }

            Channel.BindValueChanged(channelChanged);
        }

        [BackgroundDependencyLoader(true)]
        private void load(ChannelManager manager)
        {
            if (ChannelManager == null)
                ChannelManager = manager;
        }

        private void postMessage(TextBox sender, bool newtext)
        {
            var text = textbox.Text.Trim();

            if (string.IsNullOrWhiteSpace(text))
                return;

            if (text[0] == '/')
                ChannelManager?.PostCommand(text.Substring(1));
            else
                ChannelManager?.PostMessage(text);

            textbox.Text = string.Empty;
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

        protected virtual Drawable CreateMessage(Message message)
        {
            return new StandAloneMessage(message);
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
                foreach (var c in messagesFlow.Children.Take(excessChildren))
                    c.Expire();

            foreach (var message in messages)
            {
                var formatted = MessageFormatter.FormatMessage(message);
                var drawable = CreateMessage(formatted);
                drawable.Y = messagesFlow.Height;
                messagesFlow.Add(drawable);
            }
        }

        protected class StandAloneMessage : CompositeDrawable
        {
            protected readonly Message Message;
            protected OsuSpriteText SenderText;
            protected Circle ColourBox;

            public StandAloneMessage(Message message)
            {
                Message = message;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Margin = new MarginPadding(3);

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

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
                                    SenderText = new OsuSpriteText
                                    {
                                        Font = @"Exo2.0-Bold",
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        Text = Message.Sender.ToString()
                                    }
                                }
                            },
                            new Container
                            {
                                Size = new Vector2(8, OsuSpriteText.FONT_SIZE),
                                Margin = new MarginPadding { Horizontal = 3 },
                                Children = new Drawable[]
                                {
                                    ColourBox = new Circle
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Size = new Vector2(8)
                                    }
                                }
                            },
                            new OsuTextFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Width = 0.5f,
                                Text = Message.DisplayContent
                            }
                        }
                    }
                };

                if (!string.IsNullOrEmpty(Message.Sender.Colour))
                    SenderText.Colour = ColourBox.Colour = OsuColour.FromHex(Message.Sender.Colour);
            }
        }
    }
}
