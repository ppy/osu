// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Chat;
using osu.Game.Online.Chat.Drawables;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using OpenTK.Graphics;
using osu.Framework.Input;

namespace osu.Game.Overlays
{
    public class ChatOverlay : FocusedOverlayContainer, IOnlineComponent
    {
        const float textbox_height = 40;

        private DrawableChannel channelDisplay;

        private ScheduledDelegate messageRequest;

        private Container content;

        protected override Container<Drawable> Content => content;

        private FocusedTextBox inputTextBox;

        private APIAccess api;

        public ChatOverlay()
        {
            RelativeSizeAxes = Axes.X;
            Size = new Vector2(1, 300);
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;

            AddInternal(new Drawable[]
            {
                new Box
                {
                    Depth = float.MaxValue,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.9f,
                },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = 5, Bottom = textbox_height + 5 },
                },
                new Container
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = textbox_height,
                    Padding = new MarginPadding(5),
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
            });
        }

        protected override bool OnFocus(InputState state)
        {
            //this is necessary as inputTextBox is masked away and therefore can't get focus :(
            inputTextBox.TriggerFocus();
            return false;
        }

        private void postMessage(TextBox sender, bool newText)
        {
            var postText = sender.Text;

            if (!string.IsNullOrEmpty(postText))
            {
                //todo: actually send to server
                careChannels.FirstOrDefault()?.AddNewMessages(new[]
                            {
                                new Message
                                {
                                    User = api.LocalUser.Value,
                                    Timestamp = DateTimeOffset.Now,
                                    Content = postText
                                }
                            });
            }

            sender.Text = string.Empty;
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            this.api = api;
            api.Register(this);
        }

        private long? lastMessageId;

        private List<Channel> careChannels;

        private void addChannel(Channel channel)
        {
            Add(channelDisplay = new DrawableChannel(channel));
            careChannels.Add(channel);
        }

        private GetMessagesRequest fetchReq;

        public void FetchNewMessages(APIAccess api)
        {
            if (fetchReq != null) return;

            fetchReq = new GetMessagesRequest(careChannels, lastMessageId);
            fetchReq.Success += delegate (List<Message> messages)
            {
                var ids = messages.Select(m => m.ChannelId).Distinct();

                //batch messages per channel.
                foreach (var id in ids)
                    careChannels.Find(c => c.Id == id)?.AddNewMessages(messages.Where(m => m.ChannelId == id));

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

        private const int transition_length = 500;

        protected override void PopIn()
        {
            MoveToY(0, transition_length, EasingTypes.OutQuint);
            FadeIn(transition_length, EasingTypes.OutQuint);
        }

        protected override void PopOut()
        {
            MoveToY(DrawSize.Y, transition_length, EasingTypes.InSine);
            FadeOut(transition_length, EasingTypes.InSine);
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

        private void initializeChannels()
        {
            Clear();

            careChannels = new List<Channel>();

            //if (api.State != APIAccess.APIState.Online)
            //  return;

            SpriteText loading;
            Add(loading = new OsuSpriteText
            {
                Text = @"Loading available channels...",
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
                    addChannel(channels.Find(c => c.Name == @"#osu"));
                });

                //addChannel(channels.Find(c => c.Name == @"#lobby"));
                //addChannel(channels.Find(c => c.Name == @"#english"));

                messageRequest = Scheduler.AddDelayed(() => FetchNewMessages(api), 1000, true);
            };
            api.Queue(req);
        }
    }
}
