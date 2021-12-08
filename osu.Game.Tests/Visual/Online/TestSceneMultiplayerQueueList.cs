// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match.Playlist;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual.Multiplayer;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneMultiplayerQueueList : MultiplayerTestScene
    {
        private MultiplayerQueueList playlist;

        [Cached(typeof(UserLookupCache))]
        private readonly TestUserLookupCache userLookupCache = new TestUserLookupCache();

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

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("create playlist", () =>
            {
                Child = playlist = new MultiplayerQueueList
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(500, 300),
                    Items = { BindTarget = Client.APIRoom!.Playlist }
                };
            });

            AddStep("import beatmap", () =>
            {
                beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).Wait();
                importedSet = beatmaps.GetAllUsableBeatmapSetsEnumerable(IncludedDetails.All).First();
                importedBeatmap = importedSet.Beatmaps.First(b => b.RulesetID == 0);
            });

            AddStep("change to all players mode", () => Client.ChangeSettings(new MultiplayerRoomSettings { QueueMode = QueueMode.AllPlayers }));
        }

        [Test]
        public void TestDeleteButtonHiddenWithSingleItem()
        {
            AddStep("set all players queue mode", () => Client.ChangeSettings(new MultiplayerRoomSettings { QueueMode = QueueMode.AllPlayers }));
            AddUntilStep("wait for queue mode change", () => Client.APIRoom?.QueueMode.Value == QueueMode.AllPlayers);

            assertDeleteButtonVisibility(0, false);

            addPlaylistItem(() => API.LocalUser.Value.OnlineID);
            assertDeleteButtonVisibility(0, true);

            deleteItem(1);
            assertDeleteButtonVisibility(0, false);
        }

        [Test]
        public void TestDeleteButtonHiddenInHostOnlyMode()
        {
            AddStep("set all players queue mode", () => Client.ChangeSettings(new MultiplayerRoomSettings { QueueMode = QueueMode.AllPlayers }));
            AddUntilStep("wait for queue mode change", () => Client.APIRoom?.QueueMode.Value == QueueMode.AllPlayers);
            addPlaylistItem(() => 1234);

            AddStep("set host-only queue mode", () => Client.ChangeSettings(new MultiplayerRoomSettings { QueueMode = QueueMode.HostOnly }));
            AddUntilStep("wait for queue mode change", () => Client.APIRoom?.QueueMode.Value == QueueMode.HostOnly);

            assertDeleteButtonVisibility(0, false);
        }

        [Test]
        public void TestOnlyItemOwnerHasDeleteButton()
        {
            AddStep("set all players queue mode", () => Client.ChangeSettings(new MultiplayerRoomSettings { QueueMode = QueueMode.AllPlayers }));
            AddUntilStep("wait for queue mode change", () => Client.APIRoom?.QueueMode.Value == QueueMode.AllPlayers);

            addPlaylistItem(() => API.LocalUser.Value.OnlineID);
            assertDeleteButtonVisibility(0, true);
            assertDeleteButtonVisibility(1, true);

            addPlaylistItem(() => 1234);
            assertDeleteButtonVisibility(2, false);
        }

        [Test]
        public void TestNonOwnerDoesNotHaveDeleteButton()
        {
            AddStep("set all players queue mode", () => Client.ChangeSettings(new MultiplayerRoomSettings { QueueMode = QueueMode.AllPlayers }));
            AddUntilStep("wait for queue mode change", () => Client.APIRoom?.QueueMode.Value == QueueMode.AllPlayers);

            addPlaylistItem(() => 1234);
            assertDeleteButtonVisibility(0, true);
            assertDeleteButtonVisibility(1, false);
        }

        [Test]
        public void TestSelectedItemDoesNotHaveDeleteButton()
        {
            AddStep("set all players queue mode", () => Client.ChangeSettings(new MultiplayerRoomSettings { QueueMode = QueueMode.AllPlayers }));
            AddUntilStep("wait for queue mode change", () => Client.APIRoom?.QueueMode.Value == QueueMode.AllPlayers);

            addPlaylistItem(() => API.LocalUser.Value.OnlineID);
            assertDeleteButtonVisibility(0, true);

            AddStep("set first playlist item as selected", () => playlist.SelectedItem.Value = playlist.Items[0]);
            assertDeleteButtonVisibility(0, false);
        }

        private void addPlaylistItem(Func<int> userId)
        {
            long itemId = -1;

            AddStep("add playlist item", () =>
            {
                MultiplayerPlaylistItem item = new MultiplayerPlaylistItem(new PlaylistItem
                {
                    Beatmap = { Value = importedBeatmap },
                    BeatmapID = importedBeatmap.OnlineID ?? -1,
                });

                Client.AddUserPlaylistItem(userId(), item);

                itemId = item.ID;
            });

            AddUntilStep("item arrived in playlist", () => playlist.ChildrenOfType<RearrangeableListItem<PlaylistItem>>().Any(i => i.Model.ID == itemId));
        }

        private void deleteItem(int index)
        {
            OsuRearrangeableListItem<PlaylistItem> item = null;

            AddStep($"move mouse to delete button {index}", () =>
            {
                item = playlist.ChildrenOfType<OsuRearrangeableListItem<PlaylistItem>>().ElementAt(index);
                InputManager.MoveMouseTo(item.ChildrenOfType<DrawableRoomPlaylistItem.PlaylistRemoveButton>().ElementAt(0));
            });

            AddStep("click", () => InputManager.Click(MouseButton.Left));

            AddUntilStep("item removed from playlist", () => !playlist.ChildrenOfType<RearrangeableListItem<PlaylistItem>>().Contains(item));
        }

        private void assertDeleteButtonVisibility(int index, bool visible)
            => AddUntilStep($"delete button {index} {(visible ? "is" : "is not")} visible",
                () => (playlist.ChildrenOfType<DrawableRoomPlaylistItem.PlaylistRemoveButton>().ElementAt(index).Alpha > 0) == visible);
    }
}
