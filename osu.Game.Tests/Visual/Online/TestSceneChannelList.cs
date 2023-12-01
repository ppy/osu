// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
using osu.Game.Overlays.Chat.Listing;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneChannelList : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);

        [Cached]
        private readonly Bindable<Channel> selected = new Bindable<Channel>();

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
                        new Dimension(),
                    },
                    Content = new[]
                    {
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
                    selected.Value = channel;
                };

                channelList.OnRequestLeave += channel =>
                {
                    leaveText.Text = $"OnRequestLeave: {channel.Name}";
                    leaveText.FadeOutFromOne(1000, Easing.InQuint);
                    selected.Value = channelList.ChannelListingChannel;
                    channelList.RemoveChannel(channel);
                };

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

            AddStep("Add Announce Channels", () =>
            {
                for (int i = 0; i < 2; i++)
                    channelList.AddChannel(createRandomAnnounceChannel());
            });
        }

        [Test]
        public void TestVisual()
        {
            AddStep("Unread Selected", () =>
            {
                if (validItem)
                    channelList.GetItem(selected.Value).Unread.Value = true;
            });

            AddStep("Read Selected", () =>
            {
                if (validItem)
                    channelList.GetItem(selected.Value).Unread.Value = false;
            });

            AddStep("Add Mention Selected", () =>
            {
                if (validItem)
                    channelList.GetItem(selected.Value).Mentions.Value++;
            });

            AddStep("Add 98 Mentions Selected", () =>
            {
                if (validItem)
                    channelList.GetItem(selected.Value).Mentions.Value += 98;
            });

            AddStep("Clear Mentions Selected", () =>
            {
                if (validItem)
                    channelList.GetItem(selected.Value).Mentions.Value = 0;
            });
        }

        private bool validItem => selected.Value != null && !(selected.Value is ChannelListing.ChannelListingChannel);

        private Channel createRandomPublicChannel()
        {
            int id = RNG.Next(0, 100000);
            return new Channel
            {
                Name = $"#testing-channel-{id}",
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

        private Channel createRandomAnnounceChannel()
        {
            int id = RNG.Next(0, 10000);
            return new Channel
            {
                Name = $"Announce {id}",
                Type = ChannelType.Announce,
                Id = id,
            };
        }
    }
}
