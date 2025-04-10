// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Bogus;
using MessagePack;
using NUnit.Framework;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;

namespace osu.Game.Tests.OnlinePlay
{
    [TestFixture]
    public class MultiplayerPlaylistItemTest
    {
        [SetUp]
        public void Setup()
        {
            Randomizer.Seed = new Random(1337);
        }

        [Test]
        public void TestCloneMultiplayerPlaylistItem()
        {
            var faker = new Faker<MultiplayerPlaylistItem>()
                        .StrictMode(true)
                        .RuleFor(o => o.ID, f => f.Random.Long())
                        .RuleFor(o => o.OwnerID, f => f.Random.Int())
                        .RuleFor(o => o.BeatmapID, f => f.Random.Int())
                        .RuleFor(o => o.BeatmapChecksum, f => f.Random.Hash())
                        .RuleFor(o => o.RulesetID, f => f.Random.Int())
                        .RuleFor(o => o.RequiredMods, f => f.Make(5, _ => new APIMod { Acronym = f.Random.String2(3) }))
                        .RuleFor(o => o.AllowedMods, f => f.Make(5, _ => new APIMod { Acronym = f.Random.String2(3) }))
                        .RuleFor(o => o.Expired, f => f.Random.Bool())
                        .RuleFor(o => o.PlaylistOrder, f => f.Random.UShort())
                        .RuleFor(o => o.PlayedAt, f => f.Date.RecentOffset())
                        .RuleFor(o => o.StarRating, f => f.Random.Double())
                        .RuleFor(o => o.Freestyle, f => f.Random.Bool());

            for (int i = 0; i < 100; i++)
            {
                MultiplayerPlaylistItem item = faker.Generate();
                Assert.That(MessagePackSerializer.SerializeToJson(item.Clone()), Is.EqualTo(MessagePackSerializer.SerializeToJson(item)));
            }
        }

        [Test]
        public void TestConstructFromAPIModel()
        {
            var faker = new Faker<MultiplayerPlaylistItem>()
                        .StrictMode(true)
                        .RuleFor(o => o.ID, f => f.Random.Long())
                        .RuleFor(o => o.OwnerID, f => f.Random.Int())
                        .RuleFor(o => o.BeatmapID, f => f.Random.Int())
                        .RuleFor(o => o.BeatmapChecksum, f => f.Random.Hash())
                        .RuleFor(o => o.RulesetID, f => f.Random.Int())
                        .RuleFor(o => o.RequiredMods, f => f.Make(5, _ => new APIMod { Acronym = f.Random.String2(3) }))
                        .RuleFor(o => o.AllowedMods, f => f.Make(5, _ => new APIMod { Acronym = f.Random.String2(3) }))
                        .RuleFor(o => o.Expired, f => f.Random.Bool())
                        .RuleFor(o => o.PlaylistOrder, f => f.Random.UShort())
                        .RuleFor(o => o.PlayedAt, f => f.Date.RecentOffset())
                        .RuleFor(o => o.StarRating, f => f.Random.Double())
                        .RuleFor(o => o.Freestyle, f => f.Random.Bool());

            for (int i = 0; i < 100; i++)
            {
                MultiplayerPlaylistItem initialItem = faker.Generate();
                MultiplayerPlaylistItem copiedItem = new MultiplayerPlaylistItem(new PlaylistItem(initialItem));
                Assert.That(MessagePackSerializer.SerializeToJson(copiedItem), Is.EqualTo(MessagePackSerializer.SerializeToJson(initialItem)));
            }
        }
    }
}
