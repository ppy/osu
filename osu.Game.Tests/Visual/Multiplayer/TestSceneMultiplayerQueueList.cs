// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match.Playlist;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerQueueList : MultiplayerTestScene
    {
        [Cached(typeof(UserLookupCache))]
        private readonly TestUserLookupCache userLookupCache = new TestUserLookupCache();

        private MultiplayerQueueList playlist;
        private BeatmapManager beatmaps;
        private RulesetStore rulesets;
        private BeatmapSetInfo importedSet;
        private BeatmapInfo importedBeatmap;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RulesetStore(Realm));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, Realm, rulesets, API, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);
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
                beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
                importedSet = beatmaps.GetAllUsableBeatmapSets().First();
                importedBeatmap = importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0);
            });

            AddStep("change to all players mode", () => Client.ChangeSettings(new MultiplayerRoomSettings { QueueMode = QueueMode.AllPlayers }));
        }

        [Test]
        public void TestDeleteButtonAlwaysVisibleForHost()
        {
            AddStep("set all players queue mode", () => Client.ChangeSettings(new MultiplayerRoomSettings { QueueMode = QueueMode.AllPlayers }));
            AddUntilStep("wait for queue mode change", () => Client.APIRoom?.QueueMode.Value == QueueMode.AllPlayers);

            addPlaylistItem(() => API.LocalUser.Value.OnlineID);
            assertDeleteButtonVisibility(1, true);
            addPlaylistItem(() => 1234);
            assertDeleteButtonVisibility(2, true);
        }

        [Test]
        public void TestDeleteButtonOnlyVisibleForItemOwnerIfNotHost()
        {
            AddStep("set all players queue mode", () => Client.ChangeSettings(new MultiplayerRoomSettings { QueueMode = QueueMode.AllPlayers }));
            AddUntilStep("wait for queue mode change", () => Client.APIRoom?.QueueMode.Value == QueueMode.AllPlayers);

            AddStep("join other user", () => Client.AddUser(new APIUser { Id = 1234 }));
            AddStep("set other user as host", () => Client.TransferHost(1234));

            addPlaylistItem(() => API.LocalUser.Value.OnlineID);
            assertDeleteButtonVisibility(1, true);
            addPlaylistItem(() => 1234);
            assertDeleteButtonVisibility(2, false);

            AddStep("set local user as host", () => Client.TransferHost(API.LocalUser.Value.OnlineID));
            assertDeleteButtonVisibility(1, true);
            assertDeleteButtonVisibility(2, true);
        }

        [Test]
        public void TestCurrentItemDoesNotHaveDeleteButton()
        {
            AddStep("set all players queue mode", () => Client.ChangeSettings(new MultiplayerRoomSettings { QueueMode = QueueMode.AllPlayers }));
            AddUntilStep("wait for queue mode change", () => Client.APIRoom?.QueueMode.Value == QueueMode.AllPlayers);

            addPlaylistItem(() => API.LocalUser.Value.OnlineID);

            assertDeleteButtonVisibility(0, false);
            assertDeleteButtonVisibility(1, true);

            AddStep("finish current item", () => Client.FinishCurrentItem());
            AddUntilStep("wait for next item to be selected", () => Client.Room?.Settings.PlaylistItemId == 2);
            AddUntilStep("wait for two items in playlist", () => playlist.ChildrenOfType<DrawableRoomPlaylistItem>().Count() == 2);

            assertDeleteButtonVisibility(0, false);
            assertDeleteButtonVisibility(1, false);
        }

        private void addPlaylistItem(Func<int> userId)
        {
            long itemId = -1;

            AddStep("add playlist item", () =>
            {
                MultiplayerPlaylistItem item = new MultiplayerPlaylistItem(new PlaylistItem
                {
                    Beatmap = { Value = importedBeatmap },
                    BeatmapID = importedBeatmap.OnlineID,
                });

                Client.AddUserPlaylistItem(userId(), item);

                itemId = item.ID;
            });

            AddUntilStep("item arrived in playlist", () => playlist.ChildrenOfType<RearrangeableListItem<PlaylistItem>>().Any(i => i.Model.ID == itemId));
        }

        private void assertDeleteButtonVisibility(int index, bool visible)
            => AddUntilStep($"delete button {index} {(visible ? "is" : "is not")} visible", () =>
            {
                var button = playlist.ChildrenOfType<DrawableRoomPlaylistItem.PlaylistRemoveButton>().ElementAtOrDefault(index);
                return (button?.Alpha > 0) == visible;
            });
    }
}
