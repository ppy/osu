//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Threading;
using osu.Game;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Chat;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online.Chat.Display.osu.Online.Social;

namespace osu.Desktop.Tests
{
    class TestCaseChatDisplay : TestCase
    {
        private ScheduledDelegate messageRequest;

        public override string Name => @"Chat";
        public override string Description => @"Testing API polling";

        private List<Channel> channels = new List<Channel>();
        private FlowContainer flow;

        private Scheduler scheduler = new Scheduler();

        private APIAccess api => ((OsuGameBase)Game).API;

        private long? lastMessageId;

        public override void Reset()
        {
            base.Reset();

            lastMessageId = null;

            if (api.State != APIAccess.APIState.Online)
                api.OnStateChange += delegate { initializeChannels(); };
            else
                initializeChannels();

            Add(new ScrollContainer()
            {
                Size = new Vector2(1, 0.5f),
                Children = new Drawable[]
                {
                    flow = new FlowContainer
                    {
                        Direction = FlowDirection.VerticalOnly,
                        LayoutDuration = 100,
                        LayoutEasing = EasingTypes.Out,
                        Padding = new Vector2(1, 1)
                    }
                }
            });
        }

        protected override void Update()
        {
            scheduler.Update();
            base.Update();
        }

        private void initializeChannels()
        {
            if (api.State != APIAccess.APIState.Online)
                return;

            messageRequest?.Cancel();

            ListChannelsRequest req = new ListChannelsRequest();
            req.Success += delegate (List<Channel> channels)
            {
                this.channels = channels;
                messageRequest = scheduler.AddDelayed(requestNewMessages, 1000, true);
            };
            api.Queue(req);
        }

        private void requestNewMessages()
        {
            messageRequest.Wait();

            Channel channel = channels.Find(c => c.Name == "#osu");

            GetMessagesRequest gm = new GetMessagesRequest(new List<Channel> { channel }, lastMessageId);
            gm.Success += delegate (List<Message> messages)
            {
                foreach (Message m in messages)
                {
                    //m.LineWidth = this.Size.X; //this is kinda ugly.
                    //m.Drawable.Depth = m.Id;
                    //m.Drawable.FadeInFromZero(800);

                    //flow.Add(m.Drawable);

                    //if (osu.Messages.Count > 50)
                    //{
                    //    osu.Messages[0].Drawable.Expire();
                    //    osu.Messages.RemoveAt(0);
                    //}
                    flow.Add(new ChatLine(m));
                    channel.Messages.Add(m);
                }

                lastMessageId = messages.LastOrDefault()?.Id ?? lastMessageId;

                Debug.Write("success!");
                messageRequest.Continue();
            };
            gm.Failure += delegate
            {
                Debug.Write("failure!");
                messageRequest.Continue();
            };

            api.Queue(gm);
        }
    }
}
