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
        private Container<ChatOverlayDrawableChannel> currentChannelContainer = null!;

        private readonly Dictionary<Channel, ChatOverlayDrawableChannel> loadedChannels = new Dictionary<Channel, ChatOverlayDrawableChannel>();

        protected IEnumerable<DrawableChannel> DrawableChannels => loadedChannels.Values;

        private readonly BindableFloat chatHeight = new BindableFloat();
        private bool isDraggingTopBar;
        private float dragStartChatHeight;

        private const int transition_length = 500;
        private const float default_chat_height = 0.4f;
        private const float top_bar_height = 40;
        private const float side_bar_width = 190;
        private const float chat_bar_height = 60;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [Resolved]
        private ChannelManager channelManager { get; set; } = null!;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);

        [Cached]
        private readonly Bindable<Channel> currentChannel = new Bindable<Channel>();

        private readonly IBindableList<Channel> availableChannels = new BindableList<Channel>();
        private readonly IBindableList<Channel> joinedChannels = new BindableList<Channel>();

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
                        currentChannelContainer = new Container<ChatOverlayDrawableChannel>
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
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            config.BindWith(OsuSetting.ChatDisplayHeight, chatHeight);

            chatHeight.BindValueChanged(height => { Height = height.NewValue; }, true);

            currentChannel.BindTo(channelManager.CurrentChannel);
            currentChannel.BindValueChanged(currentChannelChanged, true);

            joinedChannels.BindTo(channelManager.JoinedChannels);
            joinedChannels.BindCollectionChanged(joinedChannelsChanged, true);

            availableChannels.BindTo(channelManager.AvailableChannels);
            availableChannels.BindCollectionChanged(availableChannelsChanged, true);

            channelList.OnRequestSelect += channel => channelManager.CurrentChannel.Value = channel;
            channelList.OnRequestLeave += channel => channelManager.LeaveChannel(channel);

            channelListing.OnRequestJoin += channel => channelManager.JoinChannel(channel);
            channelListing.OnRequestLeave += channel => channelManager.LeaveChannel(channel);

            textBar.OnSearchTermsChanged += searchTerms => channelListing.SearchTerm = searchTerms;
            textBar.OnChatMessageCommitted += handleChatMessage;
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

            // null channel denotes that we should be showing the listing.
            if (newChannel == null)
            {
                currentChannel.Value = channelList.ChannelListingChannel;
                return;
            }

            if (newChannel is ChannelListing.ChannelListingChannel)
            {
                currentChannelContainer.Clear(false);
                channelListing.Show();
                textBar.ShowSearch.Value = true;
            }
            else
            {
                channelListing.Hide();
                textBar.ShowSearch.Value = false;

                if (loadedChannels.ContainsKey(newChannel))
                {
                    currentChannelContainer.Clear(false);
                    currentChannelContainer.Add(loadedChannels[newChannel]);
                }
                else
                {
                    loading.Show();

                    // Ensure the drawable channel is stored before async load to prevent double loading
                    ChatOverlayDrawableChannel drawableChannel = CreateDrawableChannel(newChannel);
                    loadedChannels.Add(newChannel, drawableChannel);

                    LoadComponentAsync(drawableChannel, loadedDrawable =>
                    {
                        // Ensure the current channel hasn't changed by the time the load completes
                        if (currentChannel.Value != loadedDrawable.Channel)
                            return;

                        // Ensure the cached reference hasn't been removed from leaving the channel
                        if (!loadedChannels.ContainsKey(loadedDrawable.Channel))
                            return;

                        currentChannelContainer.Clear(false);
                        currentChannelContainer.Add(loadedDrawable);
                        loading.Hide();
                    });
                }
            }
        }

        protected virtual ChatOverlayDrawableChannel CreateDrawableChannel(Channel newChannel) => new ChatOverlayDrawableChannel(newChannel);

        private void joinedChannelsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    IEnumerable<Channel> newChannels = filterChannels(args.NewItems);

                    foreach (var channel in newChannels)
                        channelList.AddChannel(channel);

                    break;

                case NotifyCollectionChangedAction.Remove:
                    IEnumerable<Channel> leftChannels = filterChannels(args.OldItems);

                    foreach (var channel in leftChannels)
                    {
                        channelList.RemoveChannel(channel);

                        if (loadedChannels.ContainsKey(channel))
                        {
                            ChatOverlayDrawableChannel loaded = loadedChannels[channel];
                            loadedChannels.Remove(channel);
                            // DrawableChannel removed from cache must be manually disposed
                            loaded.Dispose();
                        }
                    }

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
