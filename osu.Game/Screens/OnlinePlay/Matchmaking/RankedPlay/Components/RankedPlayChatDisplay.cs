// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Online.Multiplayer;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components
{
    public partial class RankedPlayChatDisplay : CompositeDrawable, IKeyBindingHandler<GlobalAction>
    {
        [Resolved]
        private ChannelManager? channelManager { get; set; }

        [Resolved]
        private RealmKeyBindingStore keyBindingStore { get; set; } = null!;

        private readonly MultiplayerRoom room;

        private ChatTextBox textbox = null!;
        private BubbleChatHistory chatHistory = null!;

        private Channel? channel;

        private const float width = 320;

        public RankedPlayChatDisplay(MultiplayerRoom room)
        {
            Size = new Vector2(width, 160);
            this.room = room;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                textbox = new ChatTextBox
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    RelativeSizeAxes = Axes.X,
                    Height = 30,
                    CornerRadius = 10,
                    ReleaseFocusOnCommit = true,
                    HoldFocus = false,
                    Focus = onFocusGained,
                    FocusLost = onFocusLost
                },
                new Container
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Bottom = 35 },
                    Child = chatHistory = new BubbleChatHistory
                    {
                        RelativeSizeAxes = Axes.X
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            resetPlaceholderText();
            textbox.OnCommit += onCommit;

            channel = channelManager?.JoinChannel(new Channel { Id = room.ChannelID, Type = ChannelType.Multiplayer, Name = $"#lazermp_{room.RoomID}" });
            if (channel != null)
                channel.NewMessagesArrived += onNewMessagesArrived;
        }

        private void onCommit(TextBox sender, bool newText)
        {
            string text = textbox.Text.Trim();

            if (string.IsNullOrWhiteSpace(text))
                return;

            if (text[0] == '/')
                channelManager?.PostCommand(text[1..], channel);
            else
                channelManager?.PostMessage(text, target: channel);

            textbox.Text = string.Empty;
        }

        private void onNewMessagesArrived(IEnumerable<Message> bundle)
        {
            foreach (var message in bundle)
                chatHistory.PostMessage(message.Sender, message.Content);
        }

        private void onFocusGained()
        {
            textbox.PlaceholderText = ChatStrings.InputPlaceholder;
            chatHistory.Expand();
        }

        private void onFocusLost()
        {
            resetPlaceholderText();
            chatHistory.Collapse();
        }

        private void resetPlaceholderText()
        {
            textbox.PlaceholderText = Localisation.ChatStrings.InGameInputPlaceholder(keyBindingStore.GetBindingsStringFor(GlobalAction.ToggleChatFocus));
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.Back:
                    if (textbox.HasFocus)
                    {
                        Schedule(() => textbox.KillFocus());
                        return true;
                    }

                    break;

                case GlobalAction.ToggleChatFocus:
                    if (!textbox.HasFocus)
                    {
                        Schedule(() => textbox.TakeFocus());
                        return true;
                    }

                    break;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        public void Appear()
        {
            FinishTransforms();

            this.MoveToY(150f)
                .FadeOut()
                .MoveToY(0f, 240, Easing.OutCubic)
                .FadeIn(240, Easing.OutCubic);
        }

        public TransformSequence<RankedPlayChatDisplay> Disappear()
        {
            FinishTransforms();

            return this.FadeOut(240, Easing.InOutCubic)
                       .MoveToY(150f, 240, Easing.InOutCubic);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (channel != null)
                channel.NewMessagesArrived -= onNewMessagesArrived;
        }

        private partial class ChatTextBox : StandAloneChatDisplay.ChatTextBox
        {
            protected override void LoadComplete()
            {
                base.LoadComplete();

                BackgroundFocused = Colour4.FromHex("222228");
                BackgroundUnfocused = BackgroundFocused.Opacity(0.7f);
                Placeholder.Colour = Color4.White;
            }
        }

        public partial class BubbleChatHistory : CompositeDrawable
        {
            /// <summary>
            /// Maximum number of recent messages to keep.
            /// </summary>
            private const int max_length = 10;

            /// <summary>
            /// The vertical spacing between messages.
            /// </summary>
            private const float message_spacing = 2;

            /// <summary>
            /// When in a collapsed state, the time before a newly-posted message disappears from view.
            /// </summary>
            private const float time_before_disappear = 5000;

            private readonly Container<MessageBubble> messageContainer;

            private bool expanded;

            public BubbleChatHistory()
            {
                AutoSizeAxes = Axes.Y;

                InternalChild = messageContainer = new Container<MessageBubble>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                };
            }

            /// <summary>
            /// Collapses the display such that only new messages are temporarily shown.
            /// </summary>
            public void Collapse()
            {
                expanded = false;

                foreach (var child in messageContainer.Reverse().Take(max_length).Reverse())
                {
                    // Normally we wait an amount of time to preview messages before they disappear.
                    // When quickly toggling expanded and collapsed states, we want to still consider this preview window.
                    double previewTimeRemaining = Math.Max(0, time_before_disappear - (Time.Current - child.PostTime));

                    using (BeginDelayedSequence(previewTimeRemaining))
                        child.Hide();
                }
            }

            /// <summary>
            /// Expands the display such that all historical messages are shown.
            /// </summary>
            public void Expand()
            {
                expanded = true;

                foreach (var child in messageContainer.Reverse().Take(max_length))
                    child.Show();
            }

            /// <summary>
            /// Posts a message.
            /// </summary>
            /// <param name="user">The user that posted the message.</param>
            /// <param name="content">The message content.</param>
            public void PostMessage(APIUser user, string content)
            {
                var newMessage = new MessageBubble(user, content)
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    PostTime = Time.Current
                };

                messageContainer.Add(newMessage);

                float offset = 0;

                ScheduleAfterChildren(() =>
                {
                    // Layout bubbles, pushing all others upwards to make room for the new one.
                    foreach (var child in messageContainer.Reverse())
                    {
                        child.MoveToY(-offset, 400, Easing.OutPow10);
                        offset += child.DrawHeight + message_spacing;
                    }
                });

                // Hide any overflowing message.
                // Only need to handle the most-recently-overflowing one, because others would be handled in prior calls to this method.
                if (messageContainer.Count > max_length)
                {
                    var lastBubble = messageContainer[messageContainer.Count - max_length - 1];

                    lastBubble.Hide();
                    lastBubble.Expire();
                }

                newMessage.Show();

                // If not in the expanded state, hide the new message after a short while.
                if (!expanded)
                {
                    using (BeginDelayedSequence(time_before_disappear))
                        newMessage.Hide();
                }
            }

            private partial class MessageBubble : CompositeDrawable
            {
                private readonly APIUser user;
                private readonly string message;

                /// <summary>
                /// The time at which this message was posted.
                /// </summary>
                public required double PostTime { get; init; }

                public MessageBubble(APIUser user, string message)
                {
                    this.user = user;
                    this.message = message;
                    AutoSizeAxes = Axes.Both;

                    Scale = Vector2.Zero;
                    Alpha = 0;
                }

                [Resolved]
                private IAPIProvider api { get; set; } = null!;

                [BackgroundDependencyLoader]
                private void load()
                {
                    InternalChildren = new Drawable[]
                    {
                        new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            CornerRadius = 8,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = api.LocalUser.Value.Id == user.Id
                                        ? RankedPlayColourScheme.Blue.PrimaryDarkest
                                        : RankedPlayColourScheme.Red.PrimaryDarkest,
                                },
                                new Container
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Padding = new MarginPadding(8),
                                    Children = new Drawable[]
                                    {
                                        new CircularContainer
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Size = new Vector2(16),
                                            Masking = true,
                                            Child = new UpdateableAvatar(user)
                                            {
                                                DelayedLoad = false,
                                                RelativeSizeAxes = Axes.Both
                                            }
                                        },
                                        new OsuTextFlowContainer
                                        {
                                            X = 20,
                                            MaximumSize = new Vector2(width * 1.5f, 0),
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            AutoSizeAxes = Axes.Both,
                                            Text = message,
                                        }
                                    }
                                }
                            }
                        }
                    };
                }

                public override void Show()
                {
                    this.ScaleTo(1, 400, Easing.OutElasticQuarter)
                        .FadeIn(200, Easing.OutQuint);
                }

                public override void Hide()
                {
                    this.FadeOut(200, Easing.OutQuint);
                }
            }
        }
    }
}
