// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual.Multiplayer;
using osu.Game.Utils;

namespace osu.Game.Tests.Online
{
    [HeadlessTest]
    public partial class TestSceneMultiplayerBeatmapAvailabilityTracker : MultiplayerTestScene
    {
        private BeatmapManager beatmapManager = null!;
        private BeatmapInfo availableBeatmap = null!;
        private BeatmapInfo unavailableBeatmap = null!;

        private MultiplayerBeatmapAvailabilityTracker tracker = null!;

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            Dependencies.Cache(beatmapManager = new BeatmapManager(LocalStorage, Realm, null, Audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);

            beatmapManager.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();

            var importedSet = beatmapManager.GetAllUsableBeatmapSets().First();
            availableBeatmap = importedSet.Beatmaps[0];
            unavailableBeatmap = importedSet.Beatmaps[1];

            Realm.Write(r => r.Remove(r.Find<BeatmapInfo>(unavailableBeatmap.ID)!));
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("setup tracker", () =>
            {
                DummyAPIAccess api = (DummyAPIAccess)API;
                Func<APIRequest, bool>? defaultRequestHandler = api.HandleRequest;

                api.HandleRequest = req =>
                {
                    switch (req)
                    {
                        case GetBeatmapsRequest beatmapsReq:
                            var availableApiBeatmap = CreateAPIBeatmap();
                            availableApiBeatmap.OnlineID = availableBeatmap.OnlineID;
                            availableApiBeatmap.OnlineBeatmapSetID = availableBeatmap.BeatmapSet!.OnlineID;
                            availableApiBeatmap.Checksum = availableBeatmap.MD5Hash;
                            availableApiBeatmap.BeatmapSet!.OnlineID = availableBeatmap.BeatmapSet!.OnlineID;

                            var unavailableApiBeatmap = CreateAPIBeatmap();
                            unavailableApiBeatmap.OnlineID = unavailableBeatmap.OnlineID;
                            unavailableApiBeatmap.OnlineBeatmapSetID = unavailableBeatmap.BeatmapSet!.OnlineID;
                            unavailableApiBeatmap.Checksum = unavailableBeatmap.MD5Hash;
                            unavailableApiBeatmap.BeatmapSet!.OnlineID = unavailableBeatmap.BeatmapSet!.OnlineID;

                            beatmapsReq.TriggerSuccess(new GetBeatmapsResponse
                            {
                                Beatmaps = new List<APIBeatmap>
                                {
                                    availableApiBeatmap,
                                    unavailableApiBeatmap
                                }
                            });
                            return true;

                        default:
                            return defaultRequestHandler?.Invoke(req) ?? false;
                    }
                };

                Child = tracker = new MultiplayerBeatmapAvailabilityTracker();
            });
        }

        [Test]
        public void TestEnterRoomWithNotDownloadedBeatmap()
        {
            AddStep("join room", () =>
            {
                var room = CreateDefaultRoom();
                room.Playlist = [new PlaylistItem(unavailableBeatmap)];
                JoinRoom(room);
            });

            WaitForJoined();

            AddUntilStep("beatmap is not available", () => tracker.Availability.Value.State, () => Is.EqualTo(DownloadState.NotDownloaded));
        }

        [Test]
        public void TestEnterRoomWithLocallyAvailableBeatmap()
        {
            AddStep("join room", () =>
            {
                var room = CreateDefaultRoom();
                room.Playlist = [new PlaylistItem(availableBeatmap)];
                JoinRoom(room);
            });

            WaitForJoined();

            AddUntilStep("beatmap is available", () => tracker.Availability.Value.State, () => Is.EqualTo(DownloadState.LocallyAvailable));
        }

        [Test]
        public void TestAvailabilityUpdatesOnItemEdit()
        {
            AddStep("join room", () =>
            {
                var room = CreateDefaultRoom();
                room.Playlist = [new PlaylistItem(availableBeatmap)];
                JoinRoom(room);
            });

            WaitForJoined();

            AddUntilStep("beatmap is available", () => tracker.Availability.Value.State, () => Is.EqualTo(DownloadState.LocallyAvailable));

            AddStep("change item to not downloaded beatmap", () =>
            {
                PlaylistItem newItem = new PlaylistItem(MultiplayerClient.ClientRoom!.CurrentPlaylistItem).With(beatmap: new Optional<IBeatmapInfo>(unavailableBeatmap));
                MultiplayerClient.EditPlaylistItem(new MultiplayerPlaylistItem(newItem)).WaitSafely();
            });

            AddUntilStep("beatmap is not available", () => tracker.Availability.Value.State, () => Is.EqualTo(DownloadState.NotDownloaded));

            AddStep("change item to downloaded beatmap", () =>
            {
                PlaylistItem newItem = new PlaylistItem(MultiplayerClient.ClientRoom!.CurrentPlaylistItem).With(beatmap: new Optional<IBeatmapInfo>(availableBeatmap));
                MultiplayerClient.EditPlaylistItem(new MultiplayerPlaylistItem(newItem)).WaitSafely();
            });

            AddUntilStep("beatmap is available", () => tracker.Availability.Value.State, () => Is.EqualTo(DownloadState.LocallyAvailable));
        }

        [Test]
        public void TestAvailabilityUpdatesOnSettingsChange()
        {
            AddStep("join room", () =>
            {
                var room = CreateDefaultRoom();
                room.Playlist = [new PlaylistItem(availableBeatmap), new PlaylistItem(unavailableBeatmap)];
                JoinRoom(room);
            });

            WaitForJoined();

            AddUntilStep("beatmap is available", () => tracker.Availability.Value.State, () => Is.EqualTo(DownloadState.LocallyAvailable));

            AddStep("change settings to not downloaded beatmap", () => MultiplayerClient.ChangeServerRoomSettings(new MultiplayerRoomSettings(MultiplayerClient.ClientAPIRoom!)
            {
                PlaylistItemId = MultiplayerClient.ServerRoom!.Playlist[1].ID
            }).WaitSafely());

            AddUntilStep("beatmap is not available", () => tracker.Availability.Value.State, () => Is.EqualTo(DownloadState.NotDownloaded));

            AddStep("change settings to downloaded beatmap", () => MultiplayerClient.ChangeServerRoomSettings(new MultiplayerRoomSettings(MultiplayerClient.ClientAPIRoom!)
            {
                PlaylistItemId = MultiplayerClient.ServerRoom!.Playlist[0].ID
            }).WaitSafely());

            AddUntilStep("beatmap is available", () => tracker.Availability.Value.State, () => Is.EqualTo(DownloadState.LocallyAvailable));
        }
    }
}
