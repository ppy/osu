// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.MathUtils;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat.Tabs;
using osu.Game.Users;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneChannelTabControl : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ChannelTabControl),
        };

        private readonly ChannelTabControl channelTabControl;

        public TestSceneChannelTabControl()
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
                    currentText = new OsuSpriteText
                    {
                        Text = "Currently selected channel:"
                    }
                }
            });

            channelTabControl.OnRequestLeave += channel => channelTabControl.RemoveChannel(channel);
            channelTabControl.Current.ValueChanged += channel => currentText.Text = "Currently selected channel: " + channel.NewValue;

            AddStep("Add random private channel", addRandomPrivateChannel);
            AddAssert("There is only one channels", () => channelTabControl.Items.Count() == 2);
            AddRepeatStep("Add 3 random private channels", addRandomPrivateChannel, 3);
            AddAssert("There are four channels", () => channelTabControl.Items.Count() == 5);
            AddStep("Add random public channel", () => addChannel(RNG.Next().ToString()));

            AddRepeatStep("Select a random channel", () => channelTabControl.Current.Value = channelTabControl.Items.ElementAt(RNG.Next(channelTabControl.Items.Count() - 1)), 20);

            Channel channelBefore = channelTabControl.Items.First();
            AddStep("set first channel", () => channelTabControl.Current.Value = channelBefore);

            AddStep("select selector tab", () => channelTabControl.Current.Value = channelTabControl.Items.Last());
            AddAssert("selector tab is active", () => channelTabControl.ChannelSelectorActive.Value);

            AddAssert("check channel unchanged", () => channelBefore == channelTabControl.Current.Value);

            AddStep("set second channel", () => channelTabControl.Current.Value = channelTabControl.Items.Skip(1).First());
            AddAssert("selector tab is inactive", () => !channelTabControl.ChannelSelectorActive.Value);

            AddUntilStep("remove all channels", () =>
            {
                var first = channelTabControl.Items.First();
                if (first is ChannelSelectorTabItem.ChannelSelectorTabChannel)
                    return true;

                channelTabControl.RemoveChannel(first);
                return false;
            });

            AddAssert("selector tab is active", () => channelTabControl.ChannelSelectorActive.Value);
        }

        private void addRandomPrivateChannel() =>
            channelTabControl.AddChannel(new Channel(new User
            {
                Id = RNG.Next(1000, 10000000),
                Username = "Test User " + RNG.Next(1000)
            }));

        private void addChannel(string name) =>
            channelTabControl.AddChannel(new Channel
            {
                Type = ChannelType.Public,
                Name = name
            });
    }
}
