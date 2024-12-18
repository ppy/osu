// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;

namespace osu.Game.Tests.OnlinePlay
{
    [TestFixture]
    public class PlaylistExtensionsTest
    {
        [Test]
        public void TestEmpty()
        {
            // mostly an extreme edge case, i.e. during room creation.
            var items = Array.Empty<PlaylistItem>();

            Assert.Multiple(() =>
            {
                Assert.That(items.GetHistoricalItems(), Is.Empty);
                Assert.That(items.GetCurrentItem(), Is.Null);
                Assert.That(items.GetUpcomingItems(), Is.Empty);
            });
        }

        [Test]
        public void TestPlaylistItemsInOrder()
        {
            var items = new[]
            {
                new PlaylistItem(new APIBeatmap { OnlineID = 1001 }) { ID = 1, PlaylistOrder = 1 },
                new PlaylistItem(new APIBeatmap { OnlineID = 1002 }) { ID = 2, PlaylistOrder = 2 },
                new PlaylistItem(new APIBeatmap { OnlineID = 1003 }) { ID = 3, PlaylistOrder = 3 },
            };

            Assert.Multiple(() =>
            {
                Assert.That(items.GetHistoricalItems(), Is.Empty);
                Assert.That(items.GetCurrentItem(), Is.EqualTo(items[0]));
                Assert.That(items.GetUpcomingItems(), Is.EquivalentTo(items));
            });
        }

        [Test]
        public void TestPlaylistItemsOutOfOrder()
        {
            var items = new[]
            {
                new PlaylistItem(new APIBeatmap { OnlineID = 1002 }) { ID = 2, PlaylistOrder = 2 },
                new PlaylistItem(new APIBeatmap { OnlineID = 1001 }) { ID = 1, PlaylistOrder = 1 },
                new PlaylistItem(new APIBeatmap { OnlineID = 1003 }) { ID = 3, PlaylistOrder = 3 },
            };

            Assert.Multiple(() =>
            {
                Assert.That(items.GetHistoricalItems(), Is.Empty);
                Assert.That(items.GetCurrentItem(), Is.EqualTo(items[1]));
                Assert.That(items.GetUpcomingItems(), Is.EquivalentTo(new[] { items[1], items[0], items[2] }));
            });
        }

        [Test]
        public void TestExpiredPlaylistItemsSkipped()
        {
            var items = new[]
            {
                new PlaylistItem(new APIBeatmap { OnlineID = 1001 }) { ID = 1, Expired = true, PlayedAt = new DateTimeOffset(2021, 12, 21, 7, 55, 0, TimeSpan.Zero) },
                new PlaylistItem(new APIBeatmap { OnlineID = 1002 }) { ID = 2, Expired = true, PlayedAt = new DateTimeOffset(2021, 12, 21, 7, 53, 0, TimeSpan.Zero) },
                new PlaylistItem(new APIBeatmap { OnlineID = 1003 }) { ID = 3, PlaylistOrder = 3 },
            };

            Assert.Multiple(() =>
            {
                Assert.That(items.GetHistoricalItems(), Is.EquivalentTo(new[] { items[1], items[0] }));
                Assert.That(items.GetCurrentItem(), Is.EqualTo(items[2]));
                Assert.That(items.GetUpcomingItems(), Is.EquivalentTo(new[] { items[2] }));
            });
        }

        [Test]
        public void TestAllItemsExpired()
        {
            var items = new[]
            {
                new PlaylistItem(new APIBeatmap { OnlineID = 1001 }) { ID = 1, Expired = true, PlayedAt = new DateTimeOffset(2021, 12, 21, 7, 55, 0, TimeSpan.Zero) },
                new PlaylistItem(new APIBeatmap { OnlineID = 1002 }) { ID = 2, Expired = true, PlayedAt = new DateTimeOffset(2021, 12, 21, 7, 53, 0, TimeSpan.Zero) },
                new PlaylistItem(new APIBeatmap { OnlineID = 1002 }) { ID = 3, Expired = true, PlayedAt = new DateTimeOffset(2021, 12, 21, 7, 57, 0, TimeSpan.Zero) },
            };

            Assert.Multiple(() =>
            {
                Assert.That(items.GetHistoricalItems(), Is.EquivalentTo(new[] { items[1], items[0], items[2] }));
                // if all items are expired, the last-played item is expected to be returned.
                Assert.That(items.GetCurrentItem(), Is.EqualTo(items[2]));
                Assert.That(items.GetUpcomingItems(), Is.Empty);
            });
        }
    }
}
