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
using osu.Game.Overlays.Chat.Tabs;
using osu.Game.Users;
using OpenTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public class TestCaseChannelTabControl : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ChannelTabControl),
        };

        private readonly ChannelTabControl channelTabControl;

        public TestCaseChannelTabControl()
        {
            SpriteText currentText;
            Add(new Container
            {
                RelativeSizeAxes = Axes.X,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Children = new Drawable[]
                {
                    channelTabControl = new ChannelTabControl
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
                        Text = "Currently selected channel:"
                    }
                }
            });

            channelTabControl.OnRequestLeave += channel => channelTabControl.RemoveChannel(channel);
            channelTabControl.Current.ValueChanged += channel => currentText.Text = "Currently selected channel: " + channel.ToString();

            AddStep("Add random private channel", addRandomUser);
            AddAssert("There is only one channels", () => channelTabControl.Items.Count() == 2);
            AddRepeatStep("Add 3 random private channels", addRandomUser, 3);
            AddAssert("There are four channels", () => channelTabControl.Items.Count() == 5);
            AddStep("Add random public channel", () => addChannel(RNG.Next().ToString()));

            AddRepeatStep("Select a random channel", () => channelTabControl.Current.Value = channelTabControl.Items.ElementAt(RNG.Next(channelTabControl.Items.Count())), 20);
        }

        private List<User> users;

        private void addRandomUser()
        {
            channelTabControl.AddChannel(new PrivateChannel
            {
                User = users?.Count > 0
                        ? users[RNG.Next(0, users.Count - 1)]
                        : new User
                        {
                            Id = RNG.Next(),
                            Username = "testuser" + RNG.Next(1000)
                        }
            });
        }

        private void addChannel(string name)
        {
            channelTabControl.AddChannel(new Channel
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
