// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
using osuTK.Input;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Online;

namespace osu.Game.Overlays
{
    public class ChatOverlay : OsuFocusedOverlayContainer, INamedOverlayComponent, IKeyBindingHandler<PlatformAction>
    {
        public string IconTexture => "Icons/Hexacons/messaging";
        public LocalisableString Title => ChatStrings.HeaderTitle;
        public LocalisableString Description => ChatStrings.HeaderDescription;

        private const float textbox_height = 60;
        private const float channel_selection_min_height = 0.3f;

        [Resolved]
        private ChannelManager channelManager { get; set; }

        private Container<DrawableChannel> currentChannelContainer;

        private readonly List<DrawableChannel> loadedChannels = new List<DrawableChannel>();

        private LoadingSpinner loading;

        private FocusedTextBox textbox;

        private const int transition_length = 500;

        public const float DEFAULT_HEIGHT = 0.4f;

        public const float TAB_AREA_HEIGHT = 50;

        protected ChannelTabControl ChannelTabControl;

        protected virtual ChannelTabControl CreateChannelTabControl() => new ChannelTabControl();

        private Container chatContainer;
        private TabsArea tabsArea;
        private Box chatBackground;
        private Box tabBackground;

        public Bindable<float> ChatHeight { get; set; }

        private Container channelSelectionContainer;
        protected ChannelSelectionOverlay ChannelSelectionOverlay;

        public override bool Contains(Vector2 screenSpacePos) => chatContainer.ReceivePositionalInputAt(screenSpacePos)
                                                                 || (ChannelSelectionOverlay.State.Value == Visibility.Visible && ChannelSelectionOverlay.ReceivePositionalInputAt(screenSpacePos));

        public ChatOverlay()
        {
            RelativeSizeAxes = Axes.Both;
            RelativePositionAxes = Axes.Both;
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, OsuColour colours, TextureStore textures)
        {
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
                        ChannelSelectionOverlay = new ChannelSelectionOverlay
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
                                new OnlineViewContainer("Sign in to chat")
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
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
                                                    ReleaseFocusOnCommit = false,
                                                    HoldFocus = true,
                                                }
                                            }
                                        },
                                        loading = new LoadingSpinner(),
                                    },
                                }
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
                                new Sprite
                                {
                                    Texture = textures.Get(IconTexture),
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Size = new Vector2(OverlayTitle.ICON_SIZE),
                                    Margin = new MarginPadding { Left = 10 },
                                },
                                ChannelTabControl = CreateChannelTabControl().With(d =>
                                {
                                    d.Anchor = Anchor.BottomLeft;
                                    d.Origin = Anchor.BottomLeft;
                                    d.RelativeSizeAxes = Axes.Both;
                                    d.OnRequestLeave = channelManager.LeaveChannel;
                                    d.IsSwitchable = true;
                                }),
                            }
                        },
                    },
                },
            };

            textbox.OnCommit += postMessage;

            ChannelTabControl.Current.ValueChanged += current => channelManager.CurrentChannel.Value = current.NewValue;
            ChannelTabControl.ChannelSelectorActive.ValueChanged += active => ChannelSelectionOverlay.State.Value = active.NewValue ? Visibility.Visible : Visibility.Hidden;
            ChannelSelectionOverlay.State.ValueChanged += state =>
            {
                // Propagate the visibility state to ChannelSelectorActive
                ChannelTabControl.ChannelSelectorActive.Value = state.NewValue == Visibility.Visible;

                if (state.NewValue == Visibility.Visible)
                {
                    textbox.HoldFocus = false;
                    if (1f - ChatHeight.Value < channel_selection_min_height)
                        this.TransformBindableTo(ChatHeight, 1f - channel_selection_min_height, 800, Easing.OutQuint);
                }
                else
                    textbox.HoldFocus = true;
            };

            ChannelSelectionOverlay.OnRequestJoin = channel => channelManager.JoinChannel(channel);
            ChannelSelectionOverlay.OnRequestLeave = channelManager.LeaveChannel;

            ChatHeight = config.GetBindable<float>(OsuSetting.ChatDisplayHeight);
            ChatHeight.BindValueChanged(height =>
            {
                chatContainer.Height = height.NewValue;
                channelSelectionContainer.Height = 1f - height.NewValue;
                tabBackground.FadeTo(height.NewValue == 1f ? 1f : 0.8f, 200);
            }, true);

            chatBackground.Colour = colours.ChatBlue;

            loading.Show();

            // This is a relatively expensive (and blocking) operation.
            // Scheduling it ensures that it won't be performed unless the user decides to open chat.
            // TODO: Refactor OsuFocusedOverlayContainer / OverlayContainer to support delayed content loading.
            Schedule(() =>
            {
                // TODO: consider scheduling bindable callbacks to not perform when overlay is not present.
                channelManager.JoinedChannels.CollectionChanged += joinedChannelsChanged;

                foreach (Channel channel in channelManager.JoinedChannels)
                    ChannelTabControl.AddChannel(channel);

                channelManager.AvailableChannels.CollectionChanged += availableChannelsChanged;
                availableChannelsChanged(null, null);

                currentChannel = channelManager.CurrentChannel.GetBoundCopy();
                currentChannel.BindValueChanged(currentChannelChanged, true);
            });
        }

        private Bindable<Channel> currentChannel;

        private void currentChannelChanged(ValueChangedEvent<Channel> e)
        {
            if (e.NewValue == null)
            {
                textbox.Current.Disabled = true;
                currentChannelContainer.Clear(false);
                ChannelSelectionOverlay.Show();
                return;
            }

            if (e.NewValue is ChannelSelectorTabItem.ChannelSelectorTabChannel)
                return;

            textbox.Current.Disabled = e.NewValue.ReadOnly;

            if (ChannelTabControl.Current.Value != e.NewValue)
                Scheduler.Add(() => ChannelTabControl.Current.Value = e.NewValue);

            var loaded = loadedChannels.Find(d => d.Channel == e.NewValue);

            if (loaded == null)
            {
                currentChannelContainer.FadeOut(500, Easing.OutQuint);
                loading.Show();

                loaded = new DrawableChannel(e.NewValue);
                loadedChannels.Add(loaded);
                LoadComponentAsync(loaded, l =>
                {
                    if (currentChannel.Value != e.NewValue)
                        return;

                    // check once more to ensure the channel hasn't since been removed from the loaded channels list (may have been left by some automated means).
                    if (!loadedChannels.Contains(loaded))
                        return;

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

            // mark channel as read when channel switched
            if (e.NewValue.Messages.Any())
                channelManager.MarkChannelAsRead(e.NewValue);
        }

        private float startDragChatHeight;
        private bool isDragging;

        protected override bool OnDragStart(DragStartEvent e)
        {
            isDragging = tabsArea.IsHovered;

            if (!isDragging)
                return base.OnDragStart(e);

            startDragChatHeight = ChatHeight.Value;
            return true;
        }

        protected override void OnDrag(DragEvent e)
        {
            if (isDragging)
            {
                float targetChatHeight = startDragChatHeight - (e.MousePosition.Y - e.MouseDownPosition.Y) / Parent.DrawSize.Y;

                // If the channel selection screen is shown, mind its minimum height
                if (ChannelSelectionOverlay.State.Value == Visibility.Visible && targetChatHeight > 1f - channel_selection_min_height)
                    targetChatHeight = 1f - channel_selection_min_height;

                ChatHeight.Value = targetChatHeight;
            }
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            isDragging = false;
            base.OnDragEnd(e);
        }

        private void selectTab(int index)
        {
            var channel = ChannelTabControl.Items
                                           .Where(tab => !(tab is ChannelSelectorTabItem.ChannelSelectorTabChannel))
                                           .ElementAtOrDefault(index);
            if (channel != null)
                ChannelTabControl.Current.Value = channel;
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.AltPressed)
            {
                switch (e.Key)
                {
                    case Key.Number1:
                    case Key.Number2:
                    case Key.Number3:
                    case Key.Number4:
                    case Key.Number5:
                    case Key.Number6:
                    case Key.Number7:
                    case Key.Number8:
                    case Key.Number9:
                        selectTab((int)e.Key - (int)Key.Number1);
                        return true;

                    case Key.Number0:
                        selectTab(9);
                        return true;
                }
            }

            return base.OnKeyDown(e);
        }

        public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            switch (e.Action)
            {
                case PlatformAction.TabNew:
                    ChannelTabControl.SelectChannelSelectorTab();
                    return true;

                case PlatformAction.TabRestore:
                    channelManager.JoinLastClosedChannel();
                    return true;

                case PlatformAction.DocumentClose:
                    channelManager.LeaveChannel(channelManager.CurrentChannel.Value);
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e)
        {
        }

        public override bool AcceptsFocus => true;

        protected override void OnFocus(FocusEvent e)
        {
            // this is necessary as textbox is masked away and therefore can't get focus :(
            textbox.TakeFocus();
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

            ChannelSelectionOverlay.Hide();

            textbox.HoldFocus = false;
            base.PopOut();
        }

        private void joinedChannelsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Channel channel in args.NewItems.Cast<Channel>())
                        ChannelTabControl.AddChannel(channel);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (Channel channel in args.OldItems.Cast<Channel>())
                    {
                        ChannelTabControl.RemoveChannel(channel);

                        var loaded = loadedChannels.Find(c => c.Channel == channel);

                        if (loaded != null)
                        {
                            // Because the container is only cleared in the async load callback of a new channel, it is forcefully cleared
                            // to ensure that the previous channel doesn't get updated after it's disposed
                            loadedChannels.Remove(loaded);
                            currentChannelContainer.Remove(loaded);
                            loaded.Dispose();
                        }
                    }

                    break;
            }
        }

        private void availableChannelsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            ChannelSelectionOverlay.UpdateAvailableChannels(channelManager.AvailableChannels);
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
            string text = textbox.Text.Trim();

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
