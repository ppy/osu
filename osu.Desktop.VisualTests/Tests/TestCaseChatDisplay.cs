//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Threading;
using osu.Game;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Chat;
using OpenTK;
using osu.Framework.Allocation;
using osu.Game.Online.Chat.Drawables;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseChatDisplay : TestCase
    {
        private ScheduledDelegate messageRequest;

        public override string Name => @"Chat";
        public override string Description => @"Testing API polling";

        FlowContainer flow;

        private Scheduler scheduler = new Scheduler();

        private APIAccess api;

        private ChannelDisplay channelDisplay;

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            this.api = api;
        }

        public override void Reset()
        {
            base.Reset();

            if (api.State != APIState.Online)
                api.OnStateChange += delegate { initializeChannels(); };
            else
                initializeChannels();
        }

        protected override void Update()
        {
            scheduler.Update();
            base.Update();
        }

        private long? lastMessageId;

        List<Channel> careChannels;

        private void initializeChannels()
        {
            careChannels = new List<Channel>();

            if (api.State != APIState.Online)
                return;

            Add(flow = new FlowContainer
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
                Scheduler.Add(delegate
                {
                    loading.FadeOut(100);
                });

                addChannel(channels.Find(c => c.Name == @"#osu"));
                addChannel(channels.Find(c => c.Name == @"#lobby"));
                addChannel(channels.Find(c => c.Name == @"#english"));

                messageRequest = scheduler.AddDelayed(() => FetchNewMessages(api), 1000, true);
            };
            api.Queue(req);
        }

        private void addChannel(Channel channel)
        {
            flow.Add(channelDisplay = new ChannelDisplay(channel)
            {
                Size = new Vector2(1, 0.3f)
            });

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
    }
}
