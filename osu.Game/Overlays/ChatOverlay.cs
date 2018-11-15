﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Collections.Specialized;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat;
using osu.Game.Overlays.Chat.Selection;
using osu.Game.Overlays.Chat.Tabs;

namespace osu.Game.Overlays
{
    public class ChatOverlay : OsuFocusedOverlayContainer
    {
        private const float textbox_height = 60;
        private const float channel_selection_min_height = 0.3f;

        private ChannelManager channelManager;

        private readonly Container<DrawableChannel> currentChannelContainer;
        private readonly List<DrawableChannel> loadedChannels = new List<DrawableChannel>();

        private readonly LoadingAnimation loading;

        private readonly FocusedTextBox textbox;

        private const int transition_length = 500;

        public const float DEFAULT_HEIGHT = 0.4f;

        public const float TAB_AREA_HEIGHT = 50;

        private readonly ChannelTabControl channelTabControl;

        private readonly Container chatContainer;
        private readonly TabsArea tabsArea;
        private readonly Box chatBackground;
        private readonly Box tabBackground;

        public Bindable<double> ChatHeight { get; set; }

        private readonly Container channelSelectionContainer;
        private readonly ChannelSelectionOverlay channelSelection;

        public override bool Contains(Vector2 screenSpacePos) => chatContainer.ReceivePositionalInputAt(screenSpacePos) || channelSelection.State == Visibility.Visible && channelSelection.ReceivePositionalInputAt(screenSpacePos);

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
                                currentChannelContainer = new Container<DrawableChannel>
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
                        tabsArea = new TabsArea
                        {
                            Children = new Drawable[]
                            {
                                tabBackground = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black,
                                },
                                channelTabControl = new ChannelTabControl
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    RelativeSizeAxes = Axes.Both,
                                    OnRequestLeave = channel => channelManager.LeaveChannel(channel)
                                },
                            }
                        },
                    },
                },
            };

            channelTabControl.Current.ValueChanged += chat => channelManager.CurrentChannel.Value = chat;
            channelTabControl.ChannelSelectorActive.ValueChanged += value => channelSelection.State = value ? Visibility.Visible : Visibility.Hidden;
            channelSelection.StateChanged += state =>
            {
                channelTabControl.ChannelSelectorActive.Value = state == Visibility.Visible;

                if (state == Visibility.Visible)
                {
                    textbox.HoldFocus = false;
                    if (1f - ChatHeight.Value < channel_selection_min_height)
                        this.TransformBindableTo(ChatHeight, 1f - channel_selection_min_height, 800, Easing.OutQuint);
                }
                else
                    textbox.HoldFocus = true;
            };

            channelSelection.OnRequestJoin = channel => channelManager.JoinChannel(channel);
            channelSelection.OnRequestLeave = channel => channelManager.LeaveChannel(channel);
        }

        private void joinedChannelsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Channel newChannel in args.NewItems)
                    {
                        channelTabControl.AddChannel(newChannel);
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Channel removedChannel in args.OldItems)
                    {
                        channelTabControl.RemoveChannel(removedChannel);
                        loadedChannels.Remove(loadedChannels.Find(c => c.Channel == removedChannel));
                    }

                    break;
            }
        }

        private void currentChannelChanged(Channel channel)
        {
            if (channel == null)
            {
                textbox.Current.Disabled = true;
                currentChannelContainer.Clear(false);
                channelTabControl.Current.Value = null;
                return;
            }

            textbox.Current.Disabled = channel.ReadOnly;

            if (channelTabControl.Current.Value != channel)
                Scheduler.Add(() => channelTabControl.Current.Value = channel);

            var loaded = loadedChannels.Find(d => d.Channel == channel);
            if (loaded == null)
            {
                currentChannelContainer.FadeOut(500, Easing.OutQuint);
                loading.Show();

                loaded = new DrawableChannel(channel);
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
                Scheduler.Add(() => currentChannelContainer.Add(loaded));
            }
        }

        private double startDragChatHeight;
        private bool isDragging;

        protected override bool OnDragStart(DragStartEvent e)
        {
            isDragging = tabsArea.IsHovered;

            if (!isDragging)
                return base.OnDragStart(e);

            startDragChatHeight = ChatHeight.Value;
            return true;
        }

        protected override bool OnDrag(DragEvent e)
        {
            if (isDragging)
            {
                double targetChatHeight = startDragChatHeight - (e.MousePosition.Y - e.MouseDownPosition.Y) / Parent.DrawSize.Y;

                // If the channel selection screen is shown, mind its minimum height
                if (channelSelection.State == Visibility.Visible && targetChatHeight > 1f - channel_selection_min_height)
                    targetChatHeight = 1f - channel_selection_min_height;

                ChatHeight.Value = targetChatHeight;
            }

            return true;
        }

        protected override bool OnDragEnd(DragEndEvent e)
        {
            isDragging = false;
            return base.OnDragEnd(e);
        }

        public override bool AcceptsFocus => true;

        protected override void OnFocus(FocusEvent e)
        {
            //this is necessary as textbox is masked away and therefore can't get focus :(
            GetContainingInputManager().ChangeFocus(textbox);
            base.OnFocus(e);
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
        private void load(OsuConfigManager config, OsuColour colours, ChannelManager channelManager)
        {
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

            this.channelManager = channelManager;
            channelManager.CurrentChannel.ValueChanged += currentChannelChanged;
            channelManager.JoinedChannels.CollectionChanged += joinedChannelsChanged;
            channelManager.AvailableChannels.CollectionChanged += availableChannelsChanged;

            //for the case that channelmanager was faster at fetching the channels than our attachment to CollectionChanged.
            channelSelection.UpdateAvailableChannels(channelManager.AvailableChannels);
            joinedChannelsChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, channelManager.JoinedChannels));
        }

        private void availableChannelsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            channelSelection.UpdateAvailableChannels(channelManager.AvailableChannels);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (channelManager != null)
            {
                channelManager.CurrentChannel.ValueChanged -= currentChannelChanged;
                channelManager.JoinedChannels.CollectionChanged -= joinedChannelsChanged;
                channelManager.AvailableChannels.CollectionChanged -= availableChannelsChanged;
            }
        }

        private void postMessage(TextBox textbox, bool newText)
        {
            var text = textbox.Text.Trim();

            if (string.IsNullOrWhiteSpace(text))
                return;

            if (text[0] == '/')
                channelManager.PostCommand(text.Substring(1));
            else
                channelManager.PostMessage(text);

            textbox.Text = string.Empty;
        }

        private class TabsArea : Container
        {
            // IsHovered is used
            public override bool HandlePositionalInput => true;

            public TabsArea()
            {
                Name = @"tabs area";
                RelativeSizeAxes = Axes.X;
                Height = TAB_AREA_HEIGHT;
            }
        }
    }
}
