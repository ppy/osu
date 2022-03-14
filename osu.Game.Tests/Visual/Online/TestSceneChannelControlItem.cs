// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Overlays.Chat.ChannelControl;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneChannelControlItem : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);

        [Cached]
        private readonly Bindable<Channel> selected = new Bindable<Channel>();

        private static readonly List<Channel> channels = new List<Channel>
        {
            createPublicChannel("#public-channel"),
            createPublicChannel("#public-channel-long-name"),
            createPrivateChannel("test user", 2),
            createPrivateChannel("test user long name", 3),
        };

        private readonly Dictionary<Channel, ControlItem> channelMap = new Dictionary<Channel, ControlItem>();

        private FillFlowContainer flow;
        private OsuSpriteText selectedText;
        private OsuSpriteText leaveText;

        [SetUp]
        public void SetUp()
        {
            Schedule(() =>
            {
                Child = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        selectedText = new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                        },
                        leaveText = new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Height = 16,
                            AlwaysPresent = true,
                        },
                        new Container
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            AutoSizeAxes = Axes.Y,
                            Width = 190,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourProvider.Background6,
                                },
                                flow = new FillFlowContainer
                                {
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(20),
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                },
                            },
                        },
                    },
                };

                selected.BindValueChanged(change =>
                {
                    selectedText.Text = $"Selected Channel: {change.NewValue?.Name ?? "[null]"}";
                }, true);

                foreach (var channel in channels)
                {
                    var item = new ControlItem(channel);
                    flow.Add(item);
                    channelMap.Add(channel, item);
                    item.OnRequestSelect += c => selected.Value = c;
                    item.OnRequestLeave += leaveChannel;
                }
            });
        }

        [Test]
        public void TestVisual()
        {
            AddStep("Unread Selected", () =>
            {
                if (selected.Value != null)
                    channelMap[selected.Value].HasUnread = true;
            });

            AddStep("Read Selected", () =>
            {
                if (selected.Value != null)
                    channelMap[selected.Value].HasUnread = false;
            });

            AddStep("Add Mention Selected", () =>
            {
                if (selected.Value != null)
                    channelMap[selected.Value].MentionCount++;
            });

            AddStep("Add 98 Mentions Selected", () =>
            {
                if (selected.Value != null)
                    channelMap[selected.Value].MentionCount += 98;
            });

            AddStep("Clear Mentions Selected", () =>
            {
                if (selected.Value != null)
                    channelMap[selected.Value].MentionCount = 0;
            });
        }

        private void leaveChannel(Channel channel)
        {
            leaveText.Text = $"OnRequestLeave: {channel.Name}";
            leaveText.FadeIn().Then().FadeOut(1000, Easing.InQuint);
        }

        private static Channel createPublicChannel(string name) =>
            new Channel { Name = name, Type = ChannelType.Public, Id = 1234 };

        private static Channel createPrivateChannel(string username, int id)
            => new Channel(new APIUser { Id = id, Username = username });
    }
}
