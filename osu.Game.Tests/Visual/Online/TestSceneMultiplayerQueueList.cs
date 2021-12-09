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
using osu.Game.Online.API.Requests.Responses;
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
                    SelectedItem = { BindTarget = Client.CurrentMatchPlayingItem },
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

            assertDeleteButtonVisibility(0, false);

            addPlaylistItem(() => API.LocalUser.Value.OnlineID);
            assertDeleteButtonVisibility(0, false);
            assertDeleteButtonVisibility(1, true);

            // Run through gameplay.
            AddStep("set state to ready", () => Client.ChangeUserState(API.LocalUser.Value.Id, MultiplayerUserState.Ready));
            AddUntilStep("local state is ready", () => Client.LocalUser?.State == MultiplayerUserState.Ready);
            AddStep("start match", () => Client.StartMatch());
            AddUntilStep("match started", () => Client.LocalUser?.State == MultiplayerUserState.WaitingForLoad);
            AddStep("set state to loaded", () => Client.ChangeUserState(API.LocalUser.Value.Id, MultiplayerUserState.Loaded));
            AddUntilStep("local state is playing", () => Client.LocalUser?.State == MultiplayerUserState.Playing);
            AddStep("set state to finished play", () => Client.ChangeUserState(API.LocalUser.Value.Id, MultiplayerUserState.FinishedPlay));
            AddUntilStep("local state is results", () => Client.LocalUser?.State == MultiplayerUserState.Results);

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
                InputManager.MoveMouseTo(item.ChildrenOfType<DrawableRoomPlaylistItem>().ElementAt(0).RemoveButton);
            });

            AddStep("click", () => InputManager.Click(MouseButton.Left));

            AddUntilStep("item removed from playlist", () => !playlist.ChildrenOfType<RearrangeableListItem<PlaylistItem>>().Contains(item));
        }

        private void assertDeleteButtonVisibility(int index, bool visible)
            => AddUntilStep($"delete button {index} {(visible ? "is" : "is not")} visible",
                () => (playlist.ChildrenOfType<DrawableRoomPlaylistItem>().ElementAt(index).RemoveButton.Alpha > 0) == visible);
    }
}
