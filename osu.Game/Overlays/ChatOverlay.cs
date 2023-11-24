// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat;
using osu.Game.Overlays.Chat.ChannelList;
using osu.Game.Overlays.Chat.Listing;

namespace osu.Game.Overlays
{
    public partial class ChatOverlay : OsuFocusedOverlayContainer, INamedOverlayComponent, IKeyBindingHandler<PlatformAction>
    {
        public IconUsage Icon => HexaconsIcons.Messaging;
        public LocalisableString Title => ChatStrings.HeaderTitle;
        public LocalisableString Description => ChatStrings.HeaderDescription;

        private ChatOverlayTopBar topBar = null!;
        private ChannelList channelList = null!;
        private LoadingLayer loading = null!;
        private ChannelListing channelListing = null!;
        private ChatTextBar textBar = null!;
        private Container<DrawableChannel> currentChannelContainer = null!;

        private readonly Dictionary<Channel, DrawableChannel> loadedChannels = new Dictionary<Channel, DrawableChannel>();

        protected IEnumerable<DrawableChannel> DrawableChannels => loadedChannels.Values;

        private readonly BindableFloat chatHeight = new BindableFloat();
        private bool isDraggingTopBar;
        private float dragStartChatHeight;

        public const float DEFAULT_HEIGHT = 0.4f;

        private const int transition_length = 500;
        private const float top_bar_height = 40;
        private const float side_bar_width = 190;
        private const float chat_bar_height = 60;

        protected override string PopInSampleName => @"UI/overlay-big-pop-in";
        protected override string PopOutSampleName => @"UI/overlay-big-pop-out";

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [Resolved]
        private ChannelManager channelManager { get; set; } = null!;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);

        [Cached]
        private readonly Bindable<Channel?> currentChannel = new Bindable<Channel?>();

        private readonly IBindableList<Channel> availableChannels = new BindableList<Channel>();
        private readonly IBindableList<Channel> joinedChannels = new BindableList<Channel>();

        public ChatOverlay()
        {
            Height = DEFAULT_HEIGHT;

            Masking = true;

            const float corner_radius = 7f;

            CornerRadius = corner_radius;

            // Hack to hide the bottom edge corner radius off-screen.
            Margin = new MarginPadding { Bottom = -corner_radius };
            Padding = new MarginPadding { Bottom = corner_radius };

            RelativeSizeAxes = Axes.Both;
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
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = top_bar_height },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background4,
                        },
                        new OnlineViewContainer("Sign in to chat")
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                channelList = new ChannelList
                                {
                                    RelativeSizeAxes = Axes.Y,
                                    Width = side_bar_width,
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Padding = new MarginPadding
                                    {
                                        Left = side_bar_width,
                                        Bottom = chat_bar_height,
                                    },
                                    Children = new Drawable[]
                                    {
                                        new PopoverContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Child = currentChannelContainer = new Container<DrawableChannel>
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                            }
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
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            config.BindWith(OsuSetting.ChatDisplayHeight, chatHeight);

            chatHeight.BindValueChanged(height => { Height = height.NewValue; }, true);

            currentChannel.BindTo(channelManager.CurrentChannel);
            joinedChannels.BindTo(channelManager.JoinedChannels);
            availableChannels.BindTo(channelManager.AvailableChannels);

            Schedule(() =>
            {
                currentChannel.BindValueChanged(currentChannelChanged, true);
                joinedChannels.BindCollectionChanged(joinedChannelsChanged, true);
                availableChannels.BindCollectionChanged(availableChannelsChanged, true);
            });

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

        public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            switch (e.Action)
            {
                case PlatformAction.TabNew:
                    currentChannel.Value = channelList.ChannelListingChannel;
                    return true;

                case PlatformAction.DocumentClose:
                    channelManager.LeaveChannel(currentChannel.Value);
                    return true;

                case PlatformAction.TabRestore:
                    channelManager.JoinLastClosedChannel();
                    return true;

                case PlatformAction.DocumentPrevious:
                    cycleChannel(-1);
                    return true;

                case PlatformAction.DocumentNext:
                    cycleChannel(1);
                    return true;

                default:
                    return false;
            }
        }

        public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e)
        {
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

            float targetChatHeight = dragStartChatHeight - (e.MousePosition.Y - e.MouseDownPosition.Y) / Parent!.DrawSize.Y;
            chatHeight.Value = targetChatHeight;
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            isDraggingTopBar = false;
            base.OnDragEnd(e);
        }

        protected override void PopIn()
        {
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

        private void currentChannelChanged(ValueChangedEvent<Channel?> channel)
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

                if (loadedChannels.TryGetValue(newChannel, out var loadedChannel))
                {
                    currentChannelContainer.Clear(false);
                    currentChannelContainer.Add(loadedChannel);
                }
                else
                {
                    loading.Show();

                    // Ensure the drawable channel is stored before async load to prevent double loading
                    DrawableChannel drawableChannel = CreateDrawableChannel(newChannel);
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

            // Mark channel as read when channel switched
            if (newChannel.Messages.Any())
                channelManager.MarkChannelAsRead(newChannel);
        }

        protected virtual DrawableChannel CreateDrawableChannel(Channel newChannel) => new DrawableChannel(newChannel);

        private void joinedChannelsChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Assert(args.NewItems != null);

                    IEnumerable<Channel> newChannels = args.NewItems.OfType<Channel>().Where(isChatChannel);

                    foreach (var channel in newChannels)
                        channelList.AddChannel(channel);

                    break;

                case NotifyCollectionChangedAction.Remove:
                    Debug.Assert(args.OldItems != null);

                    IEnumerable<Channel> leftChannels = args.OldItems.OfType<Channel>().Where(isChatChannel);

                    foreach (var channel in leftChannels)
                    {
                        channelList.RemoveChannel(channel);

                        if (loadedChannels.ContainsKey(channel))
                        {
                            DrawableChannel loaded = loadedChannels[channel];
                            loadedChannels.Remove(channel);
                            // DrawableChannel removed from cache must be manually disposed
                            loaded.Dispose();
                        }
                    }

                    break;
            }
        }

        private void availableChannelsChanged(object? sender, NotifyCollectionChangedEventArgs args)
            => channelListing.UpdateAvailableChannels(channelManager.AvailableChannels);

        private void handleChatMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            if (message[0] == '/')
                channelManager.PostCommand(message.Substring(1));
            else
                channelManager.PostMessage(message);
        }

        private void cycleChannel(int direction)
        {
            List<Channel> overlayChannels = channelList.Channels.ToList();

            if (overlayChannels.Count < 2 || currentChannel.Value == null)
                return;

            int currentIndex = overlayChannels.IndexOf(currentChannel.Value);

            currentChannel.Value = overlayChannels[(currentIndex + direction + overlayChannels.Count) % overlayChannels.Count];

            channelList.ScrollChannelIntoView(currentChannel.Value);
        }

        /// <summary>
        /// Whether a channel should be displayed in this overlay, based on its type.
        /// </summary>
        private static bool isChatChannel(Channel channel)
        {
            switch (channel.Type)
            {
                case ChannelType.Multiplayer:
                case ChannelType.Spectator:
                case ChannelType.Temporary:
                    return false;

                default:
                    return true;
            }
        }
    }
}
