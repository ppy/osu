// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
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

            var currentItem = items.GetCurrentItem();

            Assert.That(currentItem, Is.Null);
        }

        [Test]
        public void TestPlaylistItemsInOrder()
        {
            var items = new[]
            {
                new PlaylistItem { ID = 1, BeatmapID = 1001, PlaylistOrder = 1 },
                new PlaylistItem { ID = 2, BeatmapID = 1002, PlaylistOrder = 2 },
                new PlaylistItem { ID = 3, BeatmapID = 1003, PlaylistOrder = 3 },
            };

            var currentItem = items.GetCurrentItem();

            Assert.That(currentItem, Is.EqualTo(items[0]));
        }

        [Test]
        public void TestPlaylistItemsOutOfOrder()
        {
            var items = new[]
            {
                new PlaylistItem { ID = 2, BeatmapID = 1002, PlaylistOrder = 2 },
                new PlaylistItem { ID = 1, BeatmapID = 1001, PlaylistOrder = 1 },
                new PlaylistItem { ID = 3, BeatmapID = 1003, PlaylistOrder = 3 },
            };

            var currentItem = items.GetCurrentItem();

            Assert.That(currentItem, Is.EqualTo(items[1]));
        }

        [Test]
        public void TestExpiredPlaylistItemsSkipped()
        {
            var items = new[]
            {
                new PlaylistItem { ID = 2, BeatmapID = 1002, PlaylistOrder = 2, Expired = true },
                new PlaylistItem { ID = 1, BeatmapID = 1001, PlaylistOrder = 1, Expired = true },
                new PlaylistItem { ID = 3, BeatmapID = 1003, PlaylistOrder = 3 },
            };

            var currentItem = items.GetCurrentItem();

            Assert.That(currentItem, Is.EqualTo(items[2]));
        }

        [Test]
        public void TestAllItemsExpired()
        {
            var items = new[]
            {
                new PlaylistItem { ID = 2, BeatmapID = 1002, PlaylistOrder = 2, Expired = true },
                new PlaylistItem { ID = 1, BeatmapID = 1001, PlaylistOrder = 1, Expired = true },
                new PlaylistItem { ID = 3, BeatmapID = 1003, PlaylistOrder = 3, Expired = true },
            };

            var currentItem = items.GetCurrentItem();

            // if all items are expired, the last-played item is expected to be returned.
            Assert.That(currentItem, Is.EqualTo(items[2]));
        }
    }
}
