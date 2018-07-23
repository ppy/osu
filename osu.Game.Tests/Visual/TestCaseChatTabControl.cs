// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.MathUtils;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat;
using osu.Game.Overlays.Chat.Tabs;
using osu.Game.Users;
using OpenTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public class TestCaseChatTabControl : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ChatTabControl),
            typeof(ChannelTabControl)
        };

        private readonly ChatTabControl chatTabControl;

        public TestCaseChatTabControl()
        {
            SpriteText currentText;
            Add(new Container
            {
                RelativeSizeAxes = Axes.X,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Children = new Drawable[]
                {
                    chatTabControl = new ChatTabControl
                    {
                        RelativeSizeAxes = Axes.X,
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Height = 50
                    },
                    new Box
                    {
                        Colour = Color4.Black.Opacity(0.1f),
                        RelativeSizeAxes = Axes.X,
                        Height = 50,
                        Depth = -1,
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                    }
                }
            });

            Add(new Container
            {
                Origin = Anchor.TopLeft,
                Anchor = Anchor.TopLeft,
                Children = new Drawable[]
                {
                    currentText = new SpriteText
                    {
                        Text = "Currently selected chat: "
                    }
                }
            });

            chatTabControl.OnRequestLeave += chat => chatTabControl.RemoveItem(chat);
            chatTabControl.Current.ValueChanged += chat => currentText.Text = "Currently selected chat: " + chat.ToString();

            AddStep("Add random user", addRandomUser);
            AddRepeatStep("Add 3 random users", addRandomUser, 3);
            AddStep("Add random channel", () => addChannel(RNG.Next().ToString()));
        }

        private List<User> users;

        private void addRandomUser()
        {
            if (users == null || users.Count == 0)
                return;

            chatTabControl.AddItem(new PrivateChannel { User = users[RNG.Next(0, users.Count - 1)] });
        }

        private void addChannel(string name)
        {
            chatTabControl.AddItem(new Channel
            {
                Name = name
            });
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            GetUsersRequest req = new GetUsersRequest();
            req.Success += list => users = list.Select(e => e.User).ToList();

            api.Queue(req);
        }
    }
}
