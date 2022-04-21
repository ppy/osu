// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Overlays.Chat.ChannelList;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneChannelList : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);

        [Cached]
        private readonly Bindable<Channel> selected = new Bindable<Channel>();

        private OsuSpriteText selectorText;
        private OsuSpriteText selectedText;
        private OsuSpriteText leaveText;
        private ChannelList channelList;

        [SetUp]
        public void SetUp()
        {
            Schedule(() =>
            {
                Child = new GridContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.7f,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 20),
                        new Dimension(GridSizeMode.Absolute, 20),
                        new Dimension(GridSizeMode.Absolute, 20),
                        new Dimension(),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            selectorText = new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                            },
                        },
                        new Drawable[]
                        {
                            selectedText = new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                            },
                        },
                        new Drawable[]
                        {
                            leaveText = new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                            },
                        },
                        new Drawable[]
                        {
                            channelList = new ChannelList
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.Y,
                                Width = 190,
                            },
                        },
                    },
                };

                channelList.OnRequestSelect += channel =>
                {
                    channelList.SelectorActive.Value = false;
                    selected.Value = channel;
                };

                channelList.OnRequestLeave += channel =>
                {
                    leaveText.Text = $"OnRequestLeave: {channel.Name}";
                    leaveText.FadeOutFromOne(1000, Easing.InQuint);
                    selected.Value = null;
                    channelList.RemoveChannel(channel);
                };

                channelList.SelectorActive.BindValueChanged(change =>
                {
                    selectorText.Text = $"Channel Selector Active: {change.NewValue}";
                    selected.Value = null;
                }, true);

                selected.BindValueChanged(change =>
                {
                    selectedText.Text = $"Selected Channel: {change.NewValue?.Name ?? "[null]"}";
                }, true);
            });
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Add Public Channels", () =>
            {
                for (int i = 0; i < 10; i++)
                    channelList.AddChannel(createRandomPublicChannel());
            });

            AddStep("Add Private Channels", () =>
            {
                for (int i = 0; i < 10; i++)
                    channelList.AddChannel(createRandomPrivateChannel());
            });
        }

        [Test]
        public void TestVisual()
        {
            AddStep("Unread Selected", () =>
            {
                if (selected.Value != null)
                    channelList.GetItem(selected.Value).Unread.Value = true;
            });

            AddStep("Read Selected", () =>
            {
                if (selected.Value != null)
                    channelList.GetItem(selected.Value).Unread.Value = false;
            });

            AddStep("Add Mention Selected", () =>
            {
                if (selected.Value != null)
                    channelList.GetItem(selected.Value).Mentions.Value++;
            });

            AddStep("Add 98 Mentions Selected", () =>
            {
                if (selected.Value != null)
                    channelList.GetItem(selected.Value).Mentions.Value += 98;
            });

            AddStep("Clear Mentions Selected", () =>
            {
                if (selected.Value != null)
                    channelList.GetItem(selected.Value).Mentions.Value = 0;
            });
        }

        private Channel createRandomPublicChannel()
        {
            int id = RNG.Next(0, 10000);
            return new Channel
            {
                Name = $"#channel-{id}",
                Type = ChannelType.Public,
                Id = id,
            };
        }

        private Channel createRandomPrivateChannel()
        {
            int id = RNG.Next(0, 10000);
            return new Channel(new APIUser
            {
                Id = id,
                Username = $"test user {id}",
            });
        }
    }
}
