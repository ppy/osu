// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match.Playlist;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerQueueList : OsuTestScene
    {
        private QueueList list;
        private int currentItemId;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = list = new QueueList(false, false)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Width = 0.4f,
                Height = 0.6f
            };
        });

        [SetUpSteps]
        public void SetUpSteps()
        {
            // Not inside a step since this is used to affect steps added by the current test.
            currentItemId = 0;
        }

        [Test]
        public void TestItemsAddedToEndInHostOnlyMode()
        {
            changeQueueModeStep(QueueMode.HostOnly);

            // User 1.

            PlaylistItem item1 = addItemStep(1);
            assertPositionStep(item1, 0);

            PlaylistItem item2 = addItemStep(1);
            assertPositionStep(item2, 1);

            // User 2.

            PlaylistItem item3 = addItemStep(2);
            assertPositionStep(item3, 2);
        }

        [Test]
        public void TestItemsAddedToEndInAllPlayersMode()
        {
            changeQueueModeStep(QueueMode.AllPlayers);

            // User 1.

            PlaylistItem item1 = addItemStep(1);
            assertPositionStep(item1, 0);

            PlaylistItem item2 = addItemStep(1);
            assertPositionStep(item2, 1);

            // User 2.

            PlaylistItem item3 = addItemStep(2);
            assertPositionStep(item3, 2);
        }

        [Test]
        public void TestItemsInsertedInCorrectPositionInRoundRobinMode()
        {
            changeQueueModeStep(QueueMode.AllPlayersRoundRobin);

            // User 1.

            PlaylistItem item1 = addItemStep(1);
            assertPositionStep(item1, 0);

            PlaylistItem item2 = addItemStep(1);
            assertPositionStep(item2, 1);

            // User 2.

            PlaylistItem item3 = addItemStep(2);
            assertPositionStep(item3, 1);
            assertPositionStep(item2, 2);

            PlaylistItem item4 = addItemStep(2);
            assertPositionStep(item4, 3);

            PlaylistItem item5 = addItemStep(2);
            assertPositionStep(item5, 4);

            // User 1.

            // This item is added to the end rather than injected between item4 and item5, since both users have an equal number
            // of added items at this point and this user was the last of the two to add an item.
            PlaylistItem item6 = addItemStep(1);
            assertPositionStep(item6, 5);

            // User 3.

            PlaylistItem item7 = addItemStep(3);
            assertPositionStep(item7, 2);

            PlaylistItem item8 = addItemStep(3);
            assertPositionStep(item8, 5);

            PlaylistItem item9 = addItemStep(3);
            assertPositionStep(item9, 8);
        }

        [Test]
        public void TestItemsReorderedWhenQueueModeChanged()
        {
            changeQueueModeStep(QueueMode.AllPlayers);

            var items = new List<PlaylistItem>();

            for (int i = 0; i < 8; i++)
                items.Add(addItemStep(i <= 3 ? 1 : 2));

            for (int i = 0; i < 8; i++)
                assertPositionStep(items[i], i);

            changeQueueModeStep(QueueMode.AllPlayersRoundRobin);

            for (int i = 0; i < 4; i++)
            {
                assertPositionStep(items[i], i * 2); // Items by user 1.
                assertPositionStep(items[i + 4], i * 2 + 1); // Items by user 2.
            }
        }

        /// <summary>
        /// Adds a step to create a new playlist item.
        /// </summary>
        /// <param name="ownerId">The item owner.</param>
        /// <returns>The playlist item's ID.</returns>
        private PlaylistItem addItemStep(int ownerId)
        {
            var item = new PlaylistItem
            {
                ID = ++currentItemId,
                OwnerID = ownerId,
                Beatmap = { Value = new TestBeatmap(new OsuRuleset().RulesetInfo, false).BeatmapInfo }
            };

            AddStep($"add {{ item: {item.ID}, user: {ownerId} }}", () => list.Items.Add(item));

            return item;
        }

        /// <summary>
        /// Asserts the position of a given playlist item in the visual layout of the list.
        /// </summary>
        /// <param name="item">The playlist item.</param>
        /// <param name="visualIndex">The index at which the item should appear visually. The item with index 0 is at the top of the list.</param>
        private void assertPositionStep(PlaylistItem item, int visualIndex)
        {
            AddUntilStep($"item {item.ID} has pos = {visualIndex}", () =>
            {
                return this.ChildrenOfType<DrawableRoomPlaylistItem>()
                           .OrderBy(drawable => drawable.Position.Y)
                           .TakeWhile(drawable => drawable.Item.ID != item.ID)
                           .Count() == visualIndex;
            });
        }

        private void changeQueueModeStep(QueueMode newMode) => AddStep($"change queue mode to {newMode}", () => list.QueueMode.Value = newMode);
    }
}
