// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
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
    public class TestSceneMultiplayerPlaylist : MultiplayerTestScene
    {
        private MultiplayerPlaylist list;
        private BeatmapManager beatmaps;
        private RulesetStore rulesets;
        private BeatmapSetInfo importedSet;
        private BeatmapInfo importedBeatmap;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, Resources, host, Beatmap.Default));
        }

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            Child = list = new MultiplayerPlaylist
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.4f, 0.8f)
            };
        });

        [SetUpSteps]
        public new void SetUpSteps()
        {
            AddStep("import beatmap", () =>
            {
                beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).Wait();
                importedSet = beatmaps.GetAllUsableBeatmapSetsEnumerable(IncludedDetails.All).First();
                importedBeatmap = importedSet.Beatmaps.First(b => b.RulesetID == 0);
            });

            AddStep("change to all players mode", () => Client.ChangeSettings(new MultiplayerRoomSettings { QueueMode = QueueMode.AllPlayers }));
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

            AddStep("finish current item", () => Client.FinishCurrentItem());

            assertItemInHistoryListStep(1, 0);
            assertItemInQueueListStep(2, 0);
            assertItemInQueueListStep(3, 1);

            AddStep("finish current item", () => Client.FinishCurrentItem());

            assertItemInHistoryListStep(2, 0);
            assertItemInHistoryListStep(1, 1);
            assertItemInQueueListStep(3, 0);

            AddStep("finish current item", () => Client.FinishCurrentItem());

            assertItemInHistoryListStep(3, 0);
            assertItemInHistoryListStep(2, 1);
            assertItemInHistoryListStep(1, 2);
        }

        [Test]
        public void TestListsClearedWhenRoomLeft()
        {
            addItemStep();
            AddStep("finish current item", () => Client.FinishCurrentItem());

            AddStep("leave room", () => RoomManager.PartRoom());
            AddUntilStep("wait for room part", () => !RoomJoined);

            AddUntilStep("item 0 not in lists", () => !inHistoryList(0) && !inQueueList(0));
            AddUntilStep("item 1 not in lists", () => !inHistoryList(0) && !inQueueList(0));
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
                        new PlaylistItem
                        {
                            Beatmap = { Value = new TestBeatmap(Ruleset.Value).BeatmapInfo },
                            Ruleset = { Value = Ruleset.Value }
                        },
                        new PlaylistItem
                        {
                            Beatmap = { Value = new TestBeatmap(Ruleset.Value).BeatmapInfo },
                            Ruleset = { Value = Ruleset.Value },
                            Expired = true
                        }
                    }
                });
            });

            AddUntilStep("wait for room join", () => RoomJoined);

            assertItemInQueueListStep(1, 0);
            assertItemInHistoryListStep(2, 0);
        }

        /// <summary>
        /// Adds a step to create a new playlist item.
        /// </summary>
        private void addItemStep(bool expired = false) => AddStep("add item", () => Client.AddPlaylistItem(new MultiplayerPlaylistItem(new PlaylistItem
        {
            Beatmap = { Value = importedBeatmap },
            BeatmapID = importedBeatmap.OnlineID ?? -1,
            Expired = expired,
            PlayedAt = DateTimeOffset.Now
        })));

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
