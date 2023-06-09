// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match.Playlist;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneMultiplayerPlaylist : MultiplayerTestScene
    {
        private MultiplayerPlaylist list;
        private BeatmapManager beatmaps;
        private BeatmapSetInfo importedSet;
        private BeatmapInfo importedBeatmap;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, Realm, null, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);
        }

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("create list", () =>
            {
                Child = list = new MultiplayerPlaylist
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.4f, 0.8f)
                };
            });

            AddStep("import beatmap", () =>
            {
                beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
                importedSet = beatmaps.GetAllUsableBeatmapSets().First();
                importedBeatmap = importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0);
            });

            AddStep("change to all players mode", () => MultiplayerClient.ChangeSettings(new MultiplayerRoomSettings { QueueMode = QueueMode.AllPlayers }).WaitSafely());
        }

        [Test]
        public void TestNonExpiredItemsAddedToQueueList()
        {
            assertItemInQueueListStep(1, 0);

            addItemStep();
            assertItemInQueueListStep(2, 1);

            addItemStep();
            assertItemInQueueListStep(3, 2);
        }

        [Test]
        public void TestExpiredItemsAddedToHistoryList()
        {
            assertItemInQueueListStep(1, 0);

            addItemStep(true);
            assertItemInHistoryListStep(2, 0);

            addItemStep(true);
            assertItemInHistoryListStep(3, 0);
            assertItemInHistoryListStep(2, 1);

            // Initial item is still in the queue.
            assertItemInQueueListStep(1, 0);
        }

        [Test]
        public void TestExpiredItemsMoveToQueueList()
        {
            addItemStep();
            addItemStep();

            AddStep("finish current item", () => MultiplayerClient.FinishCurrentItem().WaitSafely());

            assertItemInHistoryListStep(1, 0);
            assertItemInQueueListStep(2, 0);
            assertItemInQueueListStep(3, 1);

            AddStep("finish current item", () => MultiplayerClient.FinishCurrentItem().WaitSafely());

            assertItemInHistoryListStep(2, 0);
            assertItemInHistoryListStep(1, 1);
            assertItemInQueueListStep(3, 0);

            AddStep("finish current item", () => MultiplayerClient.FinishCurrentItem().WaitSafely());

            assertItemInHistoryListStep(3, 0);
            assertItemInHistoryListStep(2, 1);
            assertItemInHistoryListStep(1, 2);
        }

        [Test]
        public void TestListsClearedWhenRoomLeft()
        {
            addItemStep();
            AddStep("finish current item", () => MultiplayerClient.FinishCurrentItem().WaitSafely());

            AddStep("leave room", () => RoomManager.PartRoom());
            AddUntilStep("wait for room part", () => !RoomJoined);

            AddUntilStep("item 0 not in lists", () => !inHistoryList(0) && !inQueueList(0));
            AddUntilStep("item 1 not in lists", () => !inHistoryList(0) && !inQueueList(0));
        }

        [Test]
        public void TestQueueTabCount()
        {
            assertQueueTabCount(1);

            addItemStep();
            assertQueueTabCount(2);

            addItemStep();
            assertQueueTabCount(3);

            AddStep("finish current item", () => MultiplayerClient.FinishCurrentItem().WaitSafely());
            assertQueueTabCount(2);

            AddStep("leave room", () => RoomManager.PartRoom());
            AddUntilStep("wait for room part", () => !RoomJoined);
            assertQueueTabCount(0);
        }

        [Ignore("Expired items are initially removed from the room.")]
        [Test]
        public void TestJoinRoomWithMixedItemsAddedInCorrectLists()
        {
            AddStep("leave room", () => RoomManager.PartRoom());
            AddUntilStep("wait for room part", () => !RoomJoined);

            AddStep("join room with items", () =>
            {
                RoomManager.CreateRoom(new Room
                {
                    Name = { Value = "test name" },
                    Playlist =
                    {
                        new PlaylistItem(new TestBeatmap(Ruleset.Value).BeatmapInfo)
                        {
                            RulesetID = Ruleset.Value.OnlineID
                        },
                        new PlaylistItem(new TestBeatmap(Ruleset.Value).BeatmapInfo)
                        {
                            RulesetID = Ruleset.Value.OnlineID,
                            Expired = true
                        }
                    }
                });
            });

            AddUntilStep("wait for room join", () => RoomJoined);

            assertItemInQueueListStep(1, 0);
            assertItemInHistoryListStep(2, 0);
        }

        [Test]
        public void TestInsertedItemDoesNotRefreshAllOthers()
        {
            AddStep("change to round robin queue mode", () => MultiplayerClient.ChangeSettings(new MultiplayerRoomSettings { QueueMode = QueueMode.AllPlayersRoundRobin }).WaitSafely());

            // Add a few items for the local user.
            addItemStep();
            addItemStep();
            addItemStep();
            addItemStep();
            addItemStep();

            DrawableRoomPlaylistItem[] drawableItems = null;
            AddStep("get drawable items", () => drawableItems = this.ChildrenOfType<DrawableRoomPlaylistItem>().ToArray());

            // Add 1 item for another user.
            AddStep("join second user", () => MultiplayerClient.AddUser(new APIUser { Id = 10 }));
            addItemStep(userId: 10);

            // New item inserted towards the top of the list.
            assertItemInQueueListStep(7, 1);
            AddAssert("all previous playlist items remained", () => drawableItems.All(this.ChildrenOfType<DrawableRoomPlaylistItem>().Contains));
        }

        /// <summary>
        /// Adds a step to create a new playlist item.
        /// </summary>
        private void addItemStep(bool expired = false, int? userId = null) => AddStep("add item", () =>
        {
            MultiplayerClient.AddUserPlaylistItem(userId ?? API.LocalUser.Value.OnlineID, TestMultiplayerClient.CreateMultiplayerPlaylistItem(new PlaylistItem(importedBeatmap)
            {
                Expired = expired,
                PlayedAt = DateTimeOffset.Now
            })).WaitSafely();
        });

        /// <summary>
        /// Asserts the position of a given playlist item in the queue list.
        /// </summary>
        /// <param name="playlistItemId">The item id.</param>
        /// <param name="visualIndex">The index at which the item should appear visually. The item with index 0 is at the top of the list.</param>
        private void assertItemInQueueListStep(int playlistItemId, int visualIndex)
        {
            changeDisplayModeStep(MultiplayerPlaylistDisplayMode.Queue);

            AddUntilStep($"{playlistItemId} in queue at pos = {visualIndex}", () =>
            {
                return !inHistoryList(playlistItemId)
                       && this.ChildrenOfType<MultiplayerQueueList>()
                              .Single()
                              .ChildrenOfType<DrawableRoomPlaylistItem>()
                              .OrderBy(drawable => drawable.Position.Y)
                              .TakeWhile(drawable => drawable.Item.ID != playlistItemId)
                              .Count() == visualIndex;
            });
        }

        /// <summary>
        /// Asserts the position of a given playlist item in the history list.
        /// </summary>
        /// <param name="playlistItemId">The item id.</param>
        /// <param name="visualIndex">The index at which the item should appear visually. The item with index 0 is at the top of the list.</param>
        private void assertItemInHistoryListStep(int playlistItemId, int visualIndex)
        {
            changeDisplayModeStep(MultiplayerPlaylistDisplayMode.History);

            AddUntilStep($"{playlistItemId} in history at pos = {visualIndex}", () =>
            {
                return !inQueueList(playlistItemId)
                       && this.ChildrenOfType<MultiplayerHistoryList>()
                              .Single()
                              .ChildrenOfType<DrawableRoomPlaylistItem>()
                              .OrderBy(drawable => drawable.Position.Y)
                              .TakeWhile(drawable => drawable.Item.ID != playlistItemId)
                              .Count() == visualIndex;
            });
        }

        private void assertQueueTabCount(int count)
        {
            string queueTabText = count > 0 ? $"Queue ({count})" : "Queue";
            AddUntilStep($"Queue tab shows \"{queueTabText}\"", () =>
            {
                return this.ChildrenOfType<OsuTabControl<MultiplayerPlaylistDisplayMode>.OsuTabItem>()
                           .Single(t => t.Value == MultiplayerPlaylistDisplayMode.Queue)
                           .ChildrenOfType<OsuSpriteText>().Single().Text == queueTabText;
            });
        }

        private void changeDisplayModeStep(MultiplayerPlaylistDisplayMode mode) => AddStep($"change list to {mode}", () => list.DisplayMode.Value = mode);

        private bool inQueueList(int playlistItemId)
        {
            return this.ChildrenOfType<MultiplayerQueueList>()
                       .Single()
                       .Items.Any(i => i.ID == playlistItemId);
        }

        private bool inHistoryList(int playlistItemId)
        {
            return this.ChildrenOfType<MultiplayerHistoryList>()
                       .Single()
                       .Items.Any(i => i.ID == playlistItemId);
        }
    }
}
