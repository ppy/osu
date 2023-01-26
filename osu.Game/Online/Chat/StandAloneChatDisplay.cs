// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
using osu.Game.Resources.Localisation.Web;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Online.Chat
{
    /// <summary>
    /// Display a chat channel in an insolated region.
    /// </summary>
    public partial class StandAloneChatDisplay : CompositeDrawable
    {
        [Cached]
        public readonly Bindable<Channel> Channel = new Bindable<Channel>();

        protected readonly ChatTextBox TextBox;

        private ChannelManager channelManager;

        private StandAloneDrawableChannel drawableChannel;

        private readonly bool postingTextBox;

        protected readonly Box Background;

        private const float text_box_height = 30;

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        /// <param name="postingTextBox">Whether a textbox for posting new messages should be displayed.</param>
        public StandAloneChatDisplay(bool postingTextBox = false)
        {
            const float corner_radius = 10;

            this.postingTextBox = postingTextBox;
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

            if (postingTextBox)
            {
                AddInternal(TextBox = new ChatTextBox
                {
                    RelativeSizeAxes = Axes.X,
                    Height = text_box_height,
                    PlaceholderText = ChatStrings.InputPlaceholder,
                    CornerRadius = corner_radius,
                    ReleaseFocusOnCommit = false,
                    HoldFocus = true,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                });

                TextBox.OnCommit += postMessage;
            }

            Channel.BindValueChanged(channelChanged);
        }

        [BackgroundDependencyLoader(true)]
        private void load(ChannelManager manager)
        {
            channelManager ??= manager;
        }

        protected virtual StandAloneDrawableChannel CreateDrawableChannel(Channel channel) =>
            new StandAloneDrawableChannel(channel);

        private void postMessage(TextBox sender, bool newText)
        {
            string text = TextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(text))
                return;

            if (text[0] == '/')
                channelManager?.PostCommand(text.Substring(1), Channel.Value);
            else
                channelManager?.PostMessage(text, target: Channel.Value);

            TextBox.Text = string.Empty;
        }

        protected virtual ChatLine CreateMessage(Message message) => new StandAloneMessage(message);

        private void channelChanged(ValueChangedEvent<Channel> e)
        {
            drawableChannel?.Expire();

            if (e.OldValue != null)
                TextBox?.Current.UnbindFrom(e.OldValue.TextBoxMessage);

            if (e.NewValue == null) return;

            TextBox?.Current.BindTo(e.NewValue.TextBoxMessage);

            drawableChannel = CreateDrawableChannel(e.NewValue);
            drawableChannel.CreateChatLineAction = CreateMessage;
            drawableChannel.Padding = new MarginPadding { Bottom = postingTextBox ? text_box_height : 0 };

            AddInternal(drawableChannel);
        }

        public partial class ChatTextBox : HistoryTextBox
        {
            protected override bool OnKeyDown(KeyDownEvent e)
            {
                // Chat text boxes are generally used in places where they retain focus, but shouldn't block interaction with other
                // elements on the same screen.
                if (!HoldFocus)
                {
                    switch (e.Key)
                    {
                        case Key.Up:
                        case Key.Down:
                            return false;
                    }
                }

                return base.OnKeyDown(e);
            }

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

        public partial class StandAloneDrawableChannel : DrawableChannel
        {
            public Func<Message, ChatLine> CreateChatLineAction;

            public StandAloneDrawableChannel(Channel channel)
                : base(channel)
            {
            }

            protected override ChatLine CreateChatLine(Message m) => CreateChatLineAction(m);

            protected override DaySeparator CreateDaySeparator(DateTimeOffset time) => new StandAloneDaySeparator(time);
        }

        protected partial class StandAloneDaySeparator : DaySeparator
        {
            protected override float TextSize => 14;
            protected override float LineHeight => 1;
            protected override float Spacing => 5;
            protected override float DateAlign => 125;

            public StandAloneDaySeparator(DateTimeOffset date)
                : base(date)
            {
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Height = 25;
                Colour = colours.Yellow;
            }
        }

        protected partial class StandAloneMessage : ChatLine
        {
            protected override float FontSize => 15;
            protected override float Spacing => 5;
            protected override float UsernameWidth => 75;

            public StandAloneMessage(Message message)
                : base(message)
            {
            }
        }
    }
}
