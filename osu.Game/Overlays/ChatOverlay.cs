// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Threading;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Chat;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics.UserInterface;
using OpenTK.Graphics;
using osu.Framework.Input;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Overlays.Chat;

namespace osu.Game.Overlays
{
    public class ChatOverlay : FocusedOverlayContainer, IOnlineComponent
    {
        private const float textbox_height = 60;

        private ScheduledDelegate messageRequest;

        private readonly Container currentChannelContainer;

        private readonly FocusedTextBox inputTextBox;

        private APIAccess api;

        private const int transition_length = 500;

        public const float DEFAULT_HEIGHT = 0.4f;

        public const float TAB_AREA_HEIGHT = 50;

        private GetMessagesRequest fetchReq;

        private readonly ChatTabControl channelTabs;

        private readonly Box chatBackground;
        private readonly Box tabBackground;

        private Bindable<double> chatHeight;

        public ChatOverlay()
        {
            RelativeSizeAxes = Axes.Both;
            RelativePositionAxes = Axes.Both;
            Size = new Vector2(1, DEFAULT_HEIGHT);
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;

            const float padding = 5;

            Children = new Drawable[]
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
                        currentChannelContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding
                            {
                                Bottom = textbox_height + padding
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
                                inputTextBox = new FocusedTextBox
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Height = 1,
                                    PlaceholderText = "type your message",
                                    Exit = () => State = Visibility.Hidden,
                                    OnCommit = postMessage,
                                    HoldFocus = true,
                                }
                            }
                        }
                    }
                },
                new Container
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
                        channelTabs = new ChatTabControl
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    }
                },
            };

            channelTabs.Current.ValueChanged += newChannel => CurrentChannel = newChannel;
        }

        protected override bool OnDragStart(InputState state)
        {
            if (channelTabs.Hovering)
                return true;

            return base.OnDragStart(state);
        }

        protected override bool OnDrag(InputState state)
        {
            chatHeight.Value = Height - state.Mouse.Delta.Y / Parent.DrawSize.Y;
            return base.OnDrag(state);
        }

        public void APIStateChanged(APIAccess api, APIState state)
        {
            switch (state)
            {
                case APIState.Online:
                    initializeChannels();
                    break;
                default:
                    messageRequest?.Cancel();
                    break;
            }
        }

        protected override bool OnFocus(InputState state)
        {
            //this is necessary as inputTextBox is masked away and therefore can't get focus :(
            inputTextBox.TriggerFocus();
            return false;
        }

        protected override void PopIn()
        {
            MoveToY(0, transition_length, EasingTypes.OutQuint);
            FadeIn(transition_length, EasingTypes.OutQuint);

            inputTextBox.HoldFocus = true;
            base.PopIn();
        }

        protected override void PopOut()
        {
            MoveToY(Height, transition_length, EasingTypes.InSine);
            FadeOut(transition_length, EasingTypes.InSine);

            inputTextBox.HoldFocus = false;
            base.PopOut();
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api, OsuConfigManager config, OsuColour colours)
        {
            this.api = api;
            api.Register(this);

            chatHeight = config.GetBindable<double>(OsuSetting.ChatDisplayHeight);
            chatHeight.ValueChanged += h =>
            {
                Height = (float)h;
                tabBackground.FadeTo(Height == 1 ? 1 : 0.8f, 200);
            };
            chatHeight.TriggerChange();

            chatBackground.Colour = colours.ChatBlue;
        }

        private long? lastMessageId;

        private List<Channel> careChannels;

        private readonly List<DrawableChannel> loadedChannels = new List<DrawableChannel>();

        private void initializeChannels()
        {
            currentChannelContainer.Clear();

            loadedChannels.Clear();

            careChannels = new List<Channel>();

            SpriteText loading;
            Add(loading = new OsuSpriteText
            {
                Text = @"initialising chat...",
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                TextSize = 40,
            });

            messageRequest?.Cancel();

            ListChannelsRequest req = new ListChannelsRequest();
            req.Success += delegate (List<Channel> channels)
            {
                Debug.Assert(careChannels.Count == 0);

                Scheduler.Add(delegate
                {
                    loading.FadeOut(100);

                    addChannel(channels.Find(c => c.Name == @"#lazer"));
                    addChannel(channels.Find(c => c.Name == @"#osu"));
                    addChannel(channels.Find(c => c.Name == @"#lobby"));
                });

                messageRequest = Scheduler.AddDelayed(fetchNewMessages, 1000, true);
            };

            api.Queue(req);
        }

        private Channel currentChannel;

        protected Channel CurrentChannel
        {
            get
            {
                return currentChannel;
            }

            set
            {
                if (currentChannel == value) return;

                if (currentChannel != null)
                    currentChannelContainer.Clear(false);

                currentChannel = value;

                var loaded = loadedChannels.Find(d => d.Channel == value);
                if (loaded == null)
                    loadedChannels.Add(loaded = new DrawableChannel(currentChannel));

                inputTextBox.Current.Disabled = currentChannel.ReadOnly;

                currentChannelContainer.Add(loaded);

                channelTabs.Current.Value = value;
            }
        }

        private void addChannel(Channel channel)
        {
            if (channel == null) return;

            var existing = careChannels.Find(c => c.Id == channel.Id);

            if (existing != null)
            {
                // if we already have this channel loaded, we don't want to make a second one.
                channel = existing;
            }
            else
            {

                careChannels.Add(channel);
                channelTabs.AddItem(channel);
            }

            // let's fetch a small number of messages to bring us up-to-date with the backlog.
            fetchInitialMessages(channel);

            if (CurrentChannel == null)
                CurrentChannel = channel;
        }

        private void fetchInitialMessages(Channel channel)
        {
            var req = new GetMessagesRequest(new List<Channel> { channel }, null);

            req.Success += delegate (List<Message> messages)
            {
                channel.AddNewMessages(messages.ToArray());
                Debug.Write("success!");
            };
            req.Failure += delegate
            {
                Debug.Write("failure!");
            };

            api.Queue(req);
        }

        private void fetchNewMessages()
        {
            if (fetchReq != null) return;

            fetchReq = new GetMessagesRequest(careChannels, lastMessageId);
            fetchReq.Success += delegate (List<Message> messages)
            {
                var ids = messages.Where(m => m.TargetType == TargetType.Channel).Select(m => m.TargetId).Distinct();

                //batch messages per channel.
                foreach (var id in ids)
                    careChannels.Find(c => c.Id == id)?.AddNewMessages(messages.Where(m => m.TargetId == id).ToArray());

                lastMessageId = messages.LastOrDefault()?.Id ?? lastMessageId;

                Debug.Write("success!");
                fetchReq = null;
            };
            fetchReq.Failure += delegate
            {
                Debug.Write("failure!");
                fetchReq = null;
            };

            api.Queue(fetchReq);
        }

        private void postMessage(TextBox textbox, bool newText)
        {
            var postText = textbox.Text;

            if (string.IsNullOrEmpty(postText))
                return;

            if (!api.IsLoggedIn)
            {
                currentChannel?.AddNewMessages(new ErrorMessage("Please login to participate in chat!"));
                textbox.Text = string.Empty;
                return;
            }

            if (currentChannel == null) return;

            if (postText[0] == '/')
            {
                // TODO: handle commands
                currentChannel.AddNewMessages(new ErrorMessage("Chat commands are not supported yet!"));
                textbox.Text = string.Empty;
                return;
            }

            var message = new Message
            {
                Sender = api.LocalUser.Value,
                Timestamp = DateTimeOffset.Now,
                TargetType = TargetType.Channel, //TODO: read this from currentChannel
                TargetId = currentChannel.Id,
                Content = postText
            };

            textbox.ReadOnly = true;
            var req = new PostMessageRequest(message);

            req.Failure += e =>
            {
                textbox.FlashColour(Color4.Red, 1000);
                textbox.ReadOnly = false;
            };

            req.Success += m =>
            {
                currentChannel.AddNewMessages(m);

                textbox.ReadOnly = false;
                textbox.Text = string.Empty;
            };

            api.Queue(req);
        }
    }
}
