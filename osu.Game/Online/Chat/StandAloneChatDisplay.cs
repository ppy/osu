// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
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

        protected readonly ChatTextBox Textbox;

        protected ChannelManager ChannelManager;

        private StandAloneDrawableChannel drawableChannel;

        private readonly bool postingTextbox;

        protected readonly Box Background;

        private const float textbox_height = 30;

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        /// <param name="postingTextbox">Whether a textbox for posting new messages should be displayed.</param>
        public StandAloneChatDisplay(bool postingTextbox = false)
        {
            const float corner_radius = 10;

            this.postingTextbox = postingTextbox;
            CornerRadius = corner_radius;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                Background = new Box
                {
                    Colour = Color4.Black,
                    Alpha = 0.8f,
                    RelativeSizeAxes = Axes.Both
                },
            };

            if (postingTextbox)
            {
                AddInternal(Textbox = new ChatTextBox
                {
                    RelativeSizeAxes = Axes.X,
                    Height = textbox_height,
                    PlaceholderText = "type your message",
                    CornerRadius = corner_radius,
                    ReleaseFocusOnCommit = false,
                    HoldFocus = true,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                });

                Textbox.OnCommit += postMessage;
            }

            Channel.BindValueChanged(channelChanged);
        }

        [BackgroundDependencyLoader(true)]
        private void load(ChannelManager manager)
        {
            ChannelManager ??= manager;
        }

        protected virtual StandAloneDrawableChannel CreateDrawableChannel(Channel channel) =>
            new StandAloneDrawableChannel(channel);

        private void postMessage(TextBox sender, bool newtext)
        {
            string text = Textbox.Text.Trim();

            if (string.IsNullOrWhiteSpace(text))
                return;

            if (text[0] == '/')
                ChannelManager?.PostCommand(text.Substring(1), Channel.Value);
            else
                ChannelManager?.PostMessage(text, target: Channel.Value);

            Textbox.Text = string.Empty;
        }

        protected virtual ChatLine CreateMessage(Message message) => new StandAloneMessage(message);

        private void channelChanged(ValueChangedEvent<Channel> e)
        {
            drawableChannel?.Expire();

            if (e.NewValue == null) return;

            drawableChannel = CreateDrawableChannel(e.NewValue);
            drawableChannel.CreateChatLineAction = CreateMessage;
            drawableChannel.Padding = new MarginPadding { Bottom = postingTextbox ? textbox_height : 0 };

            AddInternal(drawableChannel);
        }

        public class ChatTextBox : FocusedTextBox
        {
            protected override void LoadComplete()
            {
                base.LoadComplete();

                BackgroundUnfocused = new Color4(10, 10, 10, 10);
                BackgroundFocused = new Color4(10, 10, 10, 255);
            }

            protected override void OnFocusLost(FocusLostEvent e)
            {
                base.OnFocusLost(e);
                FocusLost?.Invoke();
            }

            public Action FocusLost;
        }

        public class StandAloneDrawableChannel : DrawableChannel
        {
            public Func<Message, ChatLine> CreateChatLineAction;

            protected override ChatLine CreateChatLine(Message m) => CreateChatLineAction(m);

            protected override DaySeparator CreateDaySeparator(DateTimeOffset time) => new CustomDaySeparator(time);

            public StandAloneDrawableChannel(Channel channel)
                : base(channel)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                ChatLineFlow.Padding = new MarginPadding { Horizontal = 0 };
            }

            private class CustomDaySeparator : DaySeparator
            {
                public CustomDaySeparator(DateTimeOffset time)
                    : base(time)
                {
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    Colour = colours.Yellow;
                    TextSize = 14;
                    LineHeight = 1;
                    Padding = new MarginPadding { Horizontal = 10 };
                    Margin = new MarginPadding { Vertical = 5 };
                }
            }
        }

        protected class StandAloneMessage : ChatLine
        {
            protected override float TextSize => 15;

            protected override float HorizontalPadding => 10;
            protected override float MessagePadding => 120;
            protected override float TimestampPadding => 50;

            public StandAloneMessage(Message message)
                : base(message)
            {
            }
        }
    }
}
