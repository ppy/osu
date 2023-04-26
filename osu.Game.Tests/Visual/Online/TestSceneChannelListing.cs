// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Overlays.Chat.Listing;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneChannelListing : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider overlayColours = new OverlayColourProvider(OverlayColourScheme.Pink);

        private SearchTextBox search;
        private ChannelListing listing;

        [SetUp]
        public void SetUp()
        {
            Schedule(() =>
            {
                Children = new Drawable[]
                {
                    search = new SearchTextBox
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Width = 300,
                        Margin = new MarginPadding { Top = 100 },
                    },
                    listing = new ChannelListing
                    {
                        Size = new Vector2(800, 400),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                };
                listing.Show();
                search.Current.ValueChanged += term => listing.SearchTerm = term.NewValue;
            });
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Add Join/Leave callbacks", () =>
            {
                listing.OnRequestJoin += channel => channel.Joined.Value = true;
                listing.OnRequestLeave += channel => channel.Joined.Value = false;
            });
        }

        [Test]
        public void TestAddRandomChannels()
        {
            AddStep("Add Random Channels", () =>
            {
                listing.UpdateAvailableChannels(createRandomChannels(20));
            });
        }

        private Channel createRandomChannel()
        {
            int id = RNG.Next(0, 10000);
            return new Channel
            {
                Name = $"#channel-{id}",
                Topic = RNG.Next(4) < 3 ? $"We talk about the number {id} here" : null,
                Type = ChannelType.Public,
                Id = id,
            };
        }

        private List<Channel> createRandomChannels(int num)
            => Enumerable.Range(0, num)
                         .Select(_ => createRandomChannel())
                         .ToList();
    }
}
