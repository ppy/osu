
using System;
using System.Collections.Generic;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.MathUtils;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat;
using osu.Game.Users;
using OpenTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public class TestCaseChatTabControl : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ChatTabControl),
            typeof(ChannelTabControl),
            typeof(UserTabControl),

        };

        private readonly ChatTabControl chatTabControl;
        private readonly SpriteText currentText;

        public TestCaseChatTabControl()
        {
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

            Add(new Container()
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

            AddStep("Add random user", () => addUser(RNG.Next(100000), RNG.Next().ToString()));
            AddRepeatStep("3 random users", () => addUser(RNG.Next(100000), RNG.Next().ToString()), 3);
            AddStep("Add random channel", () => addChannel(RNG.Next().ToString()));
        }

        private void addUser(long id, string name)
        {
            chatTabControl.AddItem(new Channel(new User
            {
                Id = id,
                Username = name
            }));
        }

        private void addChannel(string name)
        {
            this.chatTabControl.AddItem(new Channel
            {
                Name = name
            });
        }
    }
}
