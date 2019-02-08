// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Chat;
using osuTK.Graphics;

namespace osu.Game.Online.Chat
{
    /// <summary>
    /// Display a chat channel in an insolated region.
    /// </summary>
    public class StandAloneChatDisplay : CompositeDrawable
    {
        public readonly Bindable<Channel> Channel = new Bindable<Channel>();

        public Action Exit;

        private readonly FocusedTextBox textbox;

        protected ChannelManager ChannelManager;

        private ScrollContainer scroll;

        private DrawableChannel drawableChannel;

        private readonly bool postingTextbox;

        private const float textbox_height = 30;

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        /// <param name="postingTextbox">Whether a textbox for posting new messages should be displayed.</param>
        public StandAloneChatDisplay(bool postingTextbox = false)
        {
            this.postingTextbox = postingTextbox;
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
            };

            if (postingTextbox)
            {
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

                textbox.Exit += () => Exit?.Invoke();
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
                ChannelManager?.PostCommand(text.Substring(1), Channel);
            else
                ChannelManager?.PostMessage(text, target: Channel);

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

        protected virtual ChatLine CreateMessage(Message message) => new StandAloneMessage(message);

        private void channelChanged(Channel channel)
        {
            drawableChannel?.Expire();

            if (channel == null) return;

            AddInternal(drawableChannel = new StandAloneDrawableChannel(channel)
            {
                CreateChatLineAction = CreateMessage,
                Padding = new MarginPadding { Bottom = postingTextbox ? textbox_height : 0 }
            });
        }

        protected class StandAloneDrawableChannel : DrawableChannel
        {
            public Func<Message,ChatLine> CreateChatLineAction;

            protected override ChatLine CreateChatLine(Message m) => CreateChatLineAction(m);

            public StandAloneDrawableChannel(Channel channel)
                : base(channel)
            {
                ChatLineFlow.Padding = new MarginPadding { Horizontal = 0 };
            }
        }

        protected class StandAloneMessage : ChatLine
        {
            protected override float TextSize => 15;

            protected override float HorizontalPadding => 10;
            protected override float MessagePadding => 120;

            public StandAloneMessage(Message message) : base(message)
            {
            }
        }
    }
}
