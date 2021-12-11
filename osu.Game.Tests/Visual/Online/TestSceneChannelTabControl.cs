// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat.Tabs;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneChannelTabControl : OsuTestScene
    {
        private readonly TestTabControl channelTabControl;

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
                    channelTabControl = new TestTabControl
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
            AddAssert("There is only one channels", () => channelTabControl.Items.Count == 2);
            AddRepeatStep("Add 3 random private channels", addRandomPrivateChannel, 3);
            AddAssert("There are four channels", () => channelTabControl.Items.Count == 5);
            AddStep("Add random public channel", () => addChannel(RNG.Next().ToString()));

            AddRepeatStep("Select a random channel", () =>
            {
                List<Channel> validChannels = channelTabControl.Items.Where(c => !(c is ChannelSelectorTabItem.ChannelSelectorTabChannel)).ToList();
                channelTabControl.SelectChannel(validChannels[RNG.Next(0, validChannels.Count)]);
            }, 20);

            Channel channelBefore = null;
            AddStep("set first channel", () => channelTabControl.SelectChannel(channelBefore = channelTabControl.Items.First(c => !(c is ChannelSelectorTabItem.ChannelSelectorTabChannel))));

            AddStep("select selector tab", () => channelTabControl.SelectChannel(channelTabControl.Items.Single(c => c is ChannelSelectorTabItem.ChannelSelectorTabChannel)));
            AddAssert("selector tab is active", () => channelTabControl.ChannelSelectorActive.Value);

            AddAssert("check channel unchanged", () => channelBefore == channelTabControl.Current.Value);

            AddStep("set second channel", () => channelTabControl.SelectChannel(channelTabControl.Items.GetNext(channelBefore)));
            AddAssert("selector tab is inactive", () => !channelTabControl.ChannelSelectorActive.Value);

            AddUntilStep("remove all channels", () =>
            {
                foreach (var item in channelTabControl.Items.ToList())
                {
                    if (item is ChannelSelectorTabItem.ChannelSelectorTabChannel)
                        continue;

                    channelTabControl.RemoveChannel(item);
                    return false;
                }

                return true;
            });

            AddAssert("selector tab is active", () => channelTabControl.ChannelSelectorActive.Value);
        }

        private void addRandomPrivateChannel() =>
            channelTabControl.AddChannel(new Channel(new APIUser
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

        private class TestTabControl : ChannelTabControl
        {
            public void SelectChannel(Channel channel) => base.SelectTab(TabMap[channel]);
        }
    }
}
