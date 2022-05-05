// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat;
using osu.Game.Overlays.Chat.ChannelList;
using osu.Game.Overlays.Chat.Listing;

namespace osu.Game.Overlays
{
    public class ChatOverlayV2 : OsuFocusedOverlayContainer, INamedOverlayComponent
    {
        public string IconTexture => "Icons/Hexacons/messaging";
        public LocalisableString Title => ChatStrings.HeaderTitle;
        public LocalisableString Description => ChatStrings.HeaderDescription;

        private ChatOverlayTopBar topBar = null!;
        private ChannelList channelList = null!;
        private LoadingLayer loading = null!;
        private ChannelListing channelListing = null!;
        private ChatTextBar textBar = null!;
        private Container<DrawableChannel> currentChannelContainer = null!;

        private readonly BindableFloat chatHeight = new BindableFloat();

        private bool isDraggingTopBar;
        private float dragStartChatHeight;

        private const int transition_length = 500;
        private const float default_chat_height = 0.4f;
        private const float top_bar_height = 40;
        private const float side_bar_width = 190;
        private const float chat_bar_height = 60;

        private readonly BindableBool selectorActive = new BindableBool();

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [Resolved]
        private ChannelManager channelManager { get; set; } = null!;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);

        [Cached]
        private readonly Bindable<Channel> currentChannel = new Bindable<Channel>();

        public ChatOverlayV2()
        {
            Height = default_chat_height;

            Masking = true;

            const float corner_radius = 7f;

            CornerRadius = corner_radius;

            // Hack to hide the bottom edge corner radius off-screen.
            Margin = new MarginPadding { Bottom = -corner_radius };
            Padding = new MarginPadding { Bottom = corner_radius };

            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // Required for the pop in/out animation
            RelativePositionAxes = Axes.Both;

            Children = new Drawable[]
            {
                topBar = new ChatOverlayTopBar
                {
                    RelativeSizeAxes = Axes.X,
                    Height = top_bar_height,
                },
                channelList = new ChannelList
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = side_bar_width,
                    Padding = new MarginPadding { Top = top_bar_height },
                    SelectorActive = { BindTarget = selectorActive },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Padding = new MarginPadding
                    {
                        Top = top_bar_height,
                        Left = side_bar_width,
                        Bottom = chat_bar_height,
                    },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background4,
                        },
                        currentChannelContainer = new Container<DrawableChannel>
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        loading = new LoadingLayer(true),
                        channelListing = new ChannelListing
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    },
                },
                textBar = new ChatTextBar
                {
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Padding = new MarginPadding { Left = side_bar_width },
                    ShowSearch = { BindTarget = selectorActive },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            loading.Show();

            config.BindWith(OsuSetting.ChatDisplayHeight, chatHeight);

            chatHeight.BindValueChanged(height => { Height = height.NewValue; }, true);

            currentChannel.BindTo(channelManager.CurrentChannel);
            channelManager.CurrentChannel.BindValueChanged(currentChannelChanged, true);
            channelManager.JoinedChannels.BindCollectionChanged(joinedChannelsChanged, true);
            channelManager.AvailableChannels.BindCollectionChanged(availableChannelsChanged, true);

            channelList.OnRequestSelect += channel =>
            {
                // Manually selecting a channel should dismiss the selector
                selectorActive.Value = false;
                channelManager.CurrentChannel.Value = channel;
            };
            channelList.OnRequestLeave += channel => channelManager.LeaveChannel(channel);

            channelListing.OnRequestJoin += channel => channelManager.JoinChannel(channel);
            channelListing.OnRequestLeave += channel => channelManager.LeaveChannel(channel);

            textBar.OnSearchTermsChanged += searchTerms => channelListing.SearchTerm = searchTerms;
            textBar.OnChatMessageCommitted += handleChatMessage;

            selectorActive.BindValueChanged(v => channelListing.State.Value = v.NewValue ? Visibility.Visible : Visibility.Hidden, true);
        }

        /// <summary>
        /// Highlights a certain message in the specified channel.
        /// </summary>
        /// <param name="message">The message to highlight.</param>
        /// <param name="channel">The channel containing the message.</param>
        public void HighlightMessage(Message message, Channel channel)
        {
            Debug.Assert(channel.Id == message.ChannelId);

            if (currentChannel.Value?.Id != channel.Id)
            {
                if (!channel.Joined.Value)
                    channel = channelManager.JoinChannel(channel);

                channelManager.CurrentChannel.Value = channel;
            }

            selectorActive.Value = false;

            channel.HighlightedMessage.Value = message;

            Show();
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            isDraggingTopBar = topBar.IsHovered;

            if (!isDraggingTopBar)
                return base.OnDragStart(e);

            dragStartChatHeight = chatHeight.Value;
            return true;
        }

        protected override void OnDrag(DragEvent e)
        {
            if (!isDraggingTopBar)
                return;

            float targetChatHeight = dragStartChatHeight - (e.MousePosition.Y - e.MouseDownPosition.Y) / Parent.DrawSize.Y;
            chatHeight.Value = targetChatHeight;
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            isDraggingTopBar = false;
            base.OnDragEnd(e);
        }

        protected override void PopIn()
        {
            base.PopIn();

            this.MoveToY(0, transition_length, Easing.OutQuint);
            this.FadeIn(transition_length, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            base.PopOut();

            this.MoveToY(Height, transition_length, Easing.InSine);
            this.FadeOut(transition_length, Easing.InSine);

            textBar.TextBoxKillFocus();
        }

        protected override void OnFocus(FocusEvent e)
        {
            textBar.TextBoxTakeFocus();
            base.OnFocus(e);
        }

        private void currentChannelChanged(ValueChangedEvent<Channel> channel)
        {
            Channel? newChannel = channel.NewValue;

            loading.Show();

            // Channel is null when leaving the currently selected channel
            if (newChannel == null)
            {
                // Find another channel to switch to
                newChannel = channelManager.JoinedChannels.FirstOrDefault(c => c != channel.OldValue);

                if (newChannel == null)
                    selectorActive.Value = true;
                else
                    currentChannel.Value = newChannel;

                return;
            }

            LoadComponentAsync(new DrawableChannel(newChannel), loaded =>
            {
                currentChannelContainer.Clear();
                currentChannelContainer.Add(loaded);
                loading.Hide();
            });
        }

        private void joinedChannelsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    IEnumerable<Channel> joinedChannels = filterChannels(args.NewItems);
                    foreach (var channel in joinedChannels)
                        channelList.AddChannel(channel);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    IEnumerable<Channel> leftChannels = filterChannels(args.OldItems);
                    foreach (var channel in leftChannels)
                        channelList.RemoveChannel(channel);
                    break;
            }
        }

        private void availableChannelsChanged(object sender, NotifyCollectionChangedEventArgs args)
            => channelListing.UpdateAvailableChannels(channelManager.AvailableChannels);

        private IEnumerable<Channel> filterChannels(IList channels)
            => channels.Cast<Channel>().Where(c => c.Type == ChannelType.Public || c.Type == ChannelType.PM);

        private void handleChatMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            if (message[0] == '/')
                channelManager.PostCommand(message.Substring(1));
            else
                channelManager.PostMessage(message);
        }
    }
}
