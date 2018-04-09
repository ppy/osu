// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.MathUtils;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat;

namespace osu.Game.Overlays
{
    public class ChatOverlay : OsuFocusedOverlayContainer
    {
        private const float textbox_height = 60;
        private const float channel_selection_min_height = 0.3f;

        private ChatManager chatManager;

        private readonly Container<DrawableChat> currentChannelContainer;
        private readonly List<DrawableChat> loadedChannels = new List<DrawableChat>();

        private readonly LoadingAnimation loading;

        private readonly FocusedTextBox textbox;

        private const int transition_length = 500;

        public const float DEFAULT_HEIGHT = 0.4f;

        public const float TAB_AREA_HEIGHT = 50;

        private readonly ChannelTabControl channelTabs;
        private readonly UserChatTabControl userTabs;

        private readonly Container chatContainer;
        private readonly Container tabsArea;
        private readonly Box chatBackground;
        private readonly Box tabBackground;

        public Bindable<double> ChatHeight { get; set; }

        private readonly Container channelSelectionContainer;
        private readonly ChannelSelectionOverlay channelSelection;


        public override bool Contains(Vector2 screenSpacePos) => chatContainer.ReceiveMouseInputAt(screenSpacePos) || channelSelection.State == Visibility.Visible && channelSelection.ReceiveMouseInputAt(screenSpacePos);

        public ChatOverlay()
        {
            RelativeSizeAxes = Axes.Both;
            RelativePositionAxes = Axes.Both;
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;

            const float padding = 5;

            Children = new Drawable[]
            {
                channelSelectionContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Height = 1f - DEFAULT_HEIGHT,
                    Masking = true,
                    Children = new[]
                    {
                        channelSelection = new ChannelSelectionOverlay
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    },
                },
                chatContainer = new Container
                {
                    Name = @"chat container",
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.Both,
                    Height = DEFAULT_HEIGHT,
                    Children = new[]
                    {
                        new Container
                        {
                            Name = @"chat area",
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Top = TAB_AREA_HEIGHT },
                            Children = new Drawable[]
                            {
                                chatBackground = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                currentChannelContainer = new Container<DrawableChat>
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding
                                    {
                                        Bottom = textbox_height
                                    },
                                },
                                new Container
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    RelativeSizeAxes = Axes.X,
                                    Height = textbox_height,
                                    Padding = new MarginPadding
                                    {
                                        Top = padding * 2,
                                        Bottom = padding * 2,
                                        Left = ChatLine.LEFT_PADDING + padding * 2,
                                        Right = padding * 2,
                                    },
                                    Children = new Drawable[]
                                    {
                                        textbox = new FocusedTextBox
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Height = 1,
                                            PlaceholderText = "type your message",
                                            Exit = () => State = Visibility.Hidden,
                                            OnCommit = postMessage,
                                            ReleaseFocusOnCommit = false,
                                            HoldFocus = true,
                                        }
                                    }
                                },
                                loading = new LoadingAnimation(),
                            }
                        },
                        tabsArea = new Container
                        {
                            Name = @"tabs area",
                            RelativeSizeAxes = Axes.X,
                            Height = TAB_AREA_HEIGHT,
                            Children = new Drawable[]
                            {
                                tabBackground = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black,
                                },
                                channelTabs = new ChannelTabControl
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    OnRequestLeave = channel => chatManager.JoinedChannels.Remove(channel),
                                },
                                userTabs = new UserChatTabControl
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    OnRequestLeave = privateChat => chatManager.OpenedUserChats.Remove(privateChat),
                                }
                            }
                        },
                    },
                },
            };

            userTabs.Current.ValueChanged += user => chatManager.CurrentChat.Value = user;
            channelTabs.Current.ValueChanged += newChannel => chatManager.CurrentChat.Value = newChannel;
            channelTabs.ChannelSelectorActive.ValueChanged += value => channelSelection.State = value ? Visibility.Visible : Visibility.Hidden;
            channelSelection.StateChanged += state =>
            {
                channelTabs.ChannelSelectorActive.Value = state == Visibility.Visible;

                if (state == Visibility.Visible)
                {
                    textbox.HoldFocus = false;
                    if (1f - ChatHeight.Value < channel_selection_min_height)
                        transformChatHeightTo(1f - channel_selection_min_height, 800, Easing.OutQuint);
                }
                else
                    textbox.HoldFocus = true;
            };
            channelSelection.OnRequestJoin = channel =>
            {
                if (!chatManager.JoinedChannels.Contains(channel))
                    chatManager.JoinedChannels.Add(channel);
            };
            channelSelection.OnRequestLeave = channel => chatManager.JoinedChannels.Remove(channel);
        }

        private void availableChannelsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            channelSelection.Sections = new[]
            {
                new ChannelSection
                {
                    Header = "All Channels",
                    Channels = chatManager.AvailableChannels,
                },
            };
        }

        private void joinedChannelsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ChannelChat newChannel in args.NewItems)
                    {
                        channelTabs.AddItem(newChannel);
                        newChannel.Joined.Value = true;
                        if (chatManager.CurrentChat.Value == null)
                        {
                            chatManager.CurrentChat.Value = newChannel;
                        }

                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ChannelChat removedChannel in args.OldItems)
                    {
                        channelTabs.RemoveItem(removedChannel);
                        loadedChannels.Remove(loadedChannels.Find(c => c.Chat == removedChannel ));
                        removedChannel.Joined.Value = false;
                        if (chatManager.CurrentChat.Value == removedChannel)
                            chatManager.CurrentChat.Value = null;
                    }
                    break;
            }
        }

        private void currentChatChanged(ChatBase chat)
        {
            if (chat == null)
            {
                textbox.Current.Disabled = true;
                currentChannelContainer.Clear(false);
                return;
            }

            textbox.Current.Disabled = chat.ReadOnly;

            switch (chat)
            {
                case ChannelChat channelChat:
                    channelTabs.Current.Value = channelChat;
                    userTabs.DeselectAll();
                    break;
                case UserChat userChat:
                    userTabs.Current.Value = userChat;
                    channelTabs.DeselectAll();
                    break;
            }

            var loaded = loadedChannels.Find(d => d.Chat == chat);
            if (loaded == null)
            {
                currentChannelContainer.FadeOut(500, Easing.OutQuint);
                loading.Show();

                loaded = new DrawableChat(chat);
                loadedChannels.Add(loaded);
                LoadComponentAsync(loaded, l =>
                {
                    loading.Hide();


                    currentChannelContainer.Clear(false);
                    currentChannelContainer.Add(loaded);
                    currentChannelContainer.FadeIn(500, Easing.OutQuint);
                });
            }
            else
            {
                currentChannelContainer.Clear(false);
                currentChannelContainer.Add(loaded);
            }
        }

        private double startDragChatHeight;
        private bool isDragging;

        protected override bool OnDragStart(InputState state)
        {
            isDragging = tabsArea.IsHovered;

            if (!isDragging)
                return base.OnDragStart(state);

            startDragChatHeight = ChatHeight.Value;
            return true;
        }

        protected override bool OnDrag(InputState state)
        {
            if (isDragging)
            {
                Trace.Assert(state.Mouse.PositionMouseDown != null);

                // ReSharper disable once PossibleInvalidOperationException
                double targetChatHeight = startDragChatHeight - (state.Mouse.Position.Y - state.Mouse.PositionMouseDown.Value.Y) / Parent.DrawSize.Y;

                // If the channel selection screen is shown, mind its minimum height
                if (channelSelection.State == Visibility.Visible && targetChatHeight > 1f - channel_selection_min_height)
                    targetChatHeight = 1f - channel_selection_min_height;

                ChatHeight.Value = targetChatHeight;
            }

            return true;
        }

        protected override bool OnDragEnd(InputState state)
        {
            isDragging = false;
            return base.OnDragEnd(state);
        }

        public override bool AcceptsFocus => true;

        protected override void OnFocus(InputState state)
        {
            //this is necessary as textbox is masked away and therefore can't get focus :(
            GetContainingInputManager().ChangeFocus(textbox);
            base.OnFocus(state);
        }

        protected override void PopIn()
        {
            this.MoveToY(0, transition_length, Easing.OutQuint);
            this.FadeIn(transition_length, Easing.OutQuint);

            textbox.HoldFocus = true;
            base.PopIn();
        }

        protected override void PopOut()
        {
            this.MoveToY(Height, transition_length, Easing.InSine);
            this.FadeOut(transition_length, Easing.InSine);

            textbox.HoldFocus = false;
            base.PopOut();
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api, OsuConfigManager config, OsuColour colours, ChatManager chatManager)
        {
            api.Register(chatManager);

            ChatHeight = config.GetBindable<double>(OsuSetting.ChatDisplayHeight);
            ChatHeight.ValueChanged += h =>
            {
                chatContainer.Height = (float)h;
                channelSelectionContainer.Height = 1f - (float)h;
                tabBackground.FadeTo(h == 1 ? 1 : 0.8f, 200);
            };
            ChatHeight.TriggerChange();

            chatBackground.Colour = colours.ChatBlue;
            loading.Show();

            this.chatManager = chatManager;
            chatManager.CurrentChat.ValueChanged += currentChatChanged;
            chatManager.JoinedChannels.CollectionChanged += joinedChannelsChanged;
            chatManager.AvailableChannels.CollectionChanged += availableChannelsChanged;
            chatManager.OpenedUserChats.CollectionChanged += openedUserChatsChanged;
        }

        private void openedUserChatsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    userTabs.AddItem(args.NewItems[0] as UserChat);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    userTabs.RemoveItem(args.OldItems[0] as UserChat);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    userTabs.Clear();
                    break;
            }
        }

        private void postMessage(TextBox textbox, bool newText)
        {
            var text = textbox.Text.Trim();

            if (string.IsNullOrWhiteSpace(text))
                return;

            if (text[0] == '/')
                chatManager.PostCommand(text.Substring(1));
            else
                chatManager.PostMessage(text);

            textbox.Text = string.Empty;
        }

        private void transformChatHeightTo(double newChatHeight, double duration = 0, Easing easing = Easing.None)
        {
            this.TransformTo(this.PopulateTransform(new TransformChatHeight(), newChatHeight, duration, easing));
        }

        private class TransformChatHeight : Transform<double, ChatOverlay>
        {
            private double valueAt(double time)
            {
                if (time < StartTime) return StartValue;
                if (time >= EndTime) return EndValue;

                return Interpolation.ValueAt(time, StartValue, EndValue, StartTime, EndTime, Easing);
            }

            public override string TargetMember => "ChatHeight.Value";

            protected override void Apply(ChatOverlay d, double time) => d.ChatHeight.Value = valueAt(time);
            protected override void ReadIntoStartValue(ChatOverlay d) => StartValue = d.ChatHeight.Value;
        }
    }
}
