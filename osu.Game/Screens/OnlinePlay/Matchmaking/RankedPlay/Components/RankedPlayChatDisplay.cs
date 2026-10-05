// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays;
using osu.Game.Overlays.Chat;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;
using ClientChatStrings = osu.Game.Localisation.ChatStrings;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components
{
    public partial class RankedPlayChatDisplay : VisibilityContainer, IKeyBindingHandler<GlobalAction>, IFocusManager
    {
        [Resolved]
        private ChannelManager? channelManager { get; set; }

        [Resolved]
        private RealmKeyBindingStore keyBindingStore { get; set; } = null!;

        private IFocusManager parentFocusManager = null!;

        private readonly MultiplayerRoom room;

        private ChatTextBox textbox = null!;
        private BubbleChatHistory chatHistory = null!;

        private Channel? channel;

        private const float chatbox_width = 320;

        public RankedPlayChatDisplay(MultiplayerRoom room)
        {
            RelativeSizeAxes = Axes.Y;
            Width = chatbox_width * 1.5f;
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
                    Width = chatbox_width,
                    Height = 30,
                    CornerRadius = 10,
                    ReleaseFocusOnCommit = true,
                    HoldFocus = false,
                    Focus = onFocusGained,
                    FocusLost = onFocusLost
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            parentFocusManager = GetContainingFocusManager()!;

            resetPlaceholderText();
            textbox.OnCommit += onCommit;

            channel = channelManager?.JoinChannel(new Channel { Id = room.ChannelID, Type = ChannelType.Multiplayer, Name = $"#lazermp_{room.RoomID}" });

            if (channel == null) return;

            textbox.Current.BindTo(channel.TextBoxMessage);

            AddInternal(new ChatContextMenuContainer
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding { Bottom = 35 },
                Child = chatHistory = new BubbleChatHistory(channel)
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    RelativeSizeAxes = Axes.X
                },
            });
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
                        Schedule(() => parentFocusManager.ChangeFocus(textbox));
                        return true;
                    }

                    break;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        public void TriggerFocusContention(Drawable? triggerSource)
        {
            if (triggerSource == null || triggerSource.IsRootedAt(chatHistory))
                parentFocusManager.TriggerFocusContention(triggerSource);
        }

        public bool ChangeFocus(Drawable? potentialFocusTarget)
        {
            if (potentialFocusTarget == null || potentialFocusTarget.IsRootedAt(chatHistory))
                return parentFocusManager.ChangeFocus(potentialFocusTarget);

            return false;
        }

        protected override void PopIn()
        {
            FinishTransforms();

            this.MoveToY(150f)
                .FadeOut()
                .MoveToY(0f, 240, Easing.OutCubic)
                .FadeIn(240, Easing.OutCubic);
        }

        protected override void PopOut()
        {
            FinishTransforms();

            this.FadeOut(240, Easing.InOutCubic)
                .MoveToY(150f, 240, Easing.InOutCubic);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (channel != null)
                textbox.Current.UnbindFrom(channel.TextBoxMessage);
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

        private partial class ChatContextMenuContainer : OsuContextMenuContainer
        {
            public ChatContextMenuContainer()
            {
                Content.Anchor = Anchor.BottomRight;
                Content.Origin = Anchor.BottomRight;
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

            [Cached]
            private readonly Channel channel;

            private readonly Container<MessageBubble> messageContainer;

            private bool expanded;

            private Sample messageReceivedSample = null!;
            private double? lastSamplePlayback;

            public BubbleChatHistory(Channel channel)
            {
                AutoSizeAxes = Axes.Y;

                this.channel = channel;

                InternalChild = messageContainer = new Container<MessageBubble>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                };
            }

            [BackgroundDependencyLoader]
            private void load(AudioManager audio)
            {
                messageReceivedSample = audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/message");

                channel.NewMessagesArrived += onNewMessagesArrived;
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

            private void onNewMessagesArrived(IEnumerable<Message> bundle)
            {
                foreach (var message in bundle)
                {
                    var newMessage = new MessageBubble(message)
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

                    playSample();

                    // If not in the expanded state, hide the new message after a short while.
                    if (!expanded)
                    {
                        using (BeginDelayedSequence(time_before_disappear))
                            newMessage.Hide();
                    }
                }
            }

            private void playSample()
            {
                if (lastSamplePlayback != null && Time.Current - lastSamplePlayback < 100)
                    return;

                messageReceivedSample.Play();
                lastSamplePlayback = Time.Current;
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                channel.NewMessagesArrived -= onNewMessagesArrived;
            }

            private partial class MessageBubble : CompositeDrawable, IHasContextMenu
            {
                private readonly Message message;

                /// <summary>
                /// The time at which this message was posted.
                /// </summary>
                public required double PostTime { get; init; }

                public MessageBubble(Message message)
                {
                    this.message = message;
                    AutoSizeAxes = Axes.Both;

                    Scale = Vector2.Zero;
                    Alpha = 0;
                }

                [Resolved]
                private IAPIProvider api { get; set; } = null!;

                [Resolved]
                private Channel channel { get; set; } = null!;

                [Resolved]
                private IDialogOverlay? dialogOverlay { get; set; }

                private const int padding = 8;
                private const int avatar_offset = 20;

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
                                    Colour = getColour(),
                                },
                                new Container
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Padding = new MarginPadding(padding),
                                    Children = new Drawable[]
                                    {
                                        new CircularContainer
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Size = new Vector2(16),
                                            Masking = true,
                                            Child = new UpdateableAvatar(message.Sender)
                                            {
                                                DelayedLoad = false,
                                                RelativeSizeAxes = Axes.Both
                                            }
                                        },
                                        new OsuTextFlowContainer
                                        {
                                            X = avatar_offset,
                                            MaximumSize = new Vector2(chatbox_width * 1.5f - avatar_offset - padding * 2, 0),
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            AutoSizeAxes = Axes.Both,
                                            Text = message.Content,
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

                private Colour4 getColour()
                {
                    if (message is InfoMessage)
                        return Color4Extensions.FromHex("#240d36");

                    if (message.Sender.Id == api.LocalUser.Value.Id)
                        return RankedPlayColourScheme.BLUE.PrimaryDarkest;

                    return RankedPlayColourScheme.RED.PrimaryDarkest;
                }

                public MenuItem[]? ContextMenuItems
                {
                    get
                    {
                        if (message.Sender.Equals(APIUser.SYSTEM_USER))
                            return null;

                        if (message.Sender.Equals(api.LocalUser.Value))
                            return null;

                        return
                        [
                            new OsuMenuItem(UsersStrings.ReportButtonText, MenuItemType.Destructive, () => dialogOverlay?.Push(new ReportChatDialog(message)
                            {
                                Success = () => channel.AddNewMessages(new InfoMessage(ClientChatStrings.ReportConfirmation)),
                            }))
                        ];
                    }
                }
            }
        }
    }
}
