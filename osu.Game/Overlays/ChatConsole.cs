﻿//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Drawables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Threading;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Chat;
using osu.Game.Online.Chat.Display;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Input;
using OpenTK.Input;

namespace osu.Game.Overlays
{
    public class ChatConsole : Container
    {
        private APIAccess api => ((OsuGameBase)Game).API;

        private ChannelDisplay channelDisplay;

        private ScheduledDelegate messageRequest;

        private Container content;

        protected override Container AddTarget => content;

        public ChatConsole()
        {
            RelativeSizeAxes = Axes.X;
            Size = new Vector2(1, 300);
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;

            AddTopLevel(new Box
            {
                Depth = float.MinValue,
                RelativeSizeAxes = Axes.Both,
                Colour = new Color4(0.1f, 0.1f, 0.1f, 0.4f)
            });

            AddTopLevel(content = new Container
            {
                RelativeSizeAxes = Axes.Both,
            });
        }

        public override void Load()
        {
            base.Load();
            initializeChannels();
        }

        private long? lastMessageId;

        List<Channel> careChannels;

        private void initializeChannels()
        {
            careChannels = new List<Channel>();

            //if (api.State != APIAccess.APIState.Online)
              //  return;

            Add(new FlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FlowDirection.VerticalOnly
            });

            SpriteText loading;
            Add(loading = new SpriteText
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
                Game.Scheduler.Add(delegate
                {
                    loading.FadeOut(100);
                });

                addChannel(channels.Find(c => c.Name == @"#osu"));
                //addChannel(channels.Find(c => c.Name == @"#lobby"));
                //addChannel(channels.Find(c => c.Name == @"#english"));

                messageRequest = Game.Scheduler.AddDelayed(() => FetchNewMessages(api), 1000, true);
            };
            api.Queue(req);
        }

        private void addChannel(Channel channel)
        {
            Add(channelDisplay = new ChannelDisplay(channel));
            careChannels.Add(channel);
        }

        GetMessagesRequest fetchReq;

        public void FetchNewMessages(APIAccess api)
        {
            if (fetchReq != null) return;

            fetchReq = new GetMessagesRequest(careChannels, lastMessageId);
            fetchReq.Success += delegate (List<Message> messages)
            {
                foreach (Message m in messages)
                {
                    careChannels.Find(c => c.Id == m.ChannelId).AddNewMessages(m);
                }

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

        public ChatConsoleState State { get; private set; }

        public void SetState(ChatConsoleState state, bool instant = false)
        {
            State = state;

            int time = instant ? 0 : 500;

            switch (state)
            {
                case ChatConsoleState.Hidden:
                    MoveToY(-Size.Y, time, EasingTypes.InQuint);
                    FadeOut(time, EasingTypes.InQuint);
                    break;
                case ChatConsoleState.Visible:
                    MoveToY(0, time, EasingTypes.OutQuint);
                    FadeIn(time, EasingTypes.OutQuint);
                    break;
            }
        }
    }

    public enum ChatConsoleState
    {
        Visible,
        Hidden,
    }
}
