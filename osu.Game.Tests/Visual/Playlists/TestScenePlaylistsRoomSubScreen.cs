// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Online.Rooms.RoomStatuses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Playlists
{
    public partial class TestScenePlaylistsRoomSubScreen : OnlinePlayTestScene
    {
        private const double track_length = 10000;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        protected new TestRoomManager RoomManager => (TestRoomManager)base.RoomManager;

        private BeatmapManager beatmaps = null!;
        private RulesetStore rulesets = null!;
        private BeatmapSetInfo? importedSet;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, Realm, null, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(new ScoreManager(rulesets, () => beatmaps, LocalStorage, Realm, API));
            Dependencies.Cache(Realm);

            beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();

            Realm.Write(r =>
            {
                foreach (var set in r.All<BeatmapSetInfo>())
                {
                    foreach (var b in set.Beatmaps)
                    {
                        // These will all have a virtual track length of 1000, see WorkingBeatmap.GetVirtualTrack().
                        b.Length = track_length - 1000;
                    }
                }
            });

            importedSet = beatmaps.GetAllUsableBeatmapSets().First();
        }

        [Test]
        public void TestStatusUpdateOnEnter()
        {
            Room room = null!;
            PlaylistsRoomSubScreen roomScreen = null!;

            AddStep("create room", () =>
            {
                RoomManager.AddRoom(room = new Room
                {
                    Name = @"Test Room",
                    Host = new APIUser { Username = @"Host" },
                    Category = RoomCategory.Normal,
                    EndDate = DateTimeOffset.Now.AddMinutes(-1)
                });
            });

            AddStep("push screen", () => LoadScreen(roomScreen = new PlaylistsRoomSubScreen(room)));
            AddUntilStep("wait for screen load", () => roomScreen.IsCurrentScreen());
            AddAssert("status is still ended", () => roomScreen.Room.Status, Is.TypeOf<RoomStatusEnded>);
        }

        [Test]
        public void TestCloseButtonGoesAwayAfterGracePeriod()
        {
            Room room = null!;
            PlaylistsRoomSubScreen roomScreen = null!;

            AddStep("create room", () =>
            {
                RoomManager.AddRoom(room = new Room
                {
                    Name = @"Test Room",
                    Host = api.LocalUser.Value,
                    Category = RoomCategory.Normal,
                    StartDate = DateTimeOffset.Now.AddMinutes(-5).AddSeconds(3),
                    EndDate = DateTimeOffset.Now.AddMinutes(30)
                });
            });

            AddStep("push screen", () => LoadScreen(roomScreen = new PlaylistsRoomSubScreen(room)));
            AddUntilStep("wait for screen load", () => roomScreen.IsCurrentScreen());
            AddAssert("close button present", () => roomScreen.ChildrenOfType<DangerousRoundedButton>().Any());
            AddUntilStep("wait for close button to disappear", () => !roomScreen.ChildrenOfType<DangerousRoundedButton>().Any());
        }

        [TestCase(120_000, true)] // Definitely enough time.
        [TestCase(45_000, true)] // Enough time.
        [TestCase(35_000, false)] // Not enough time to complete beatmap after lenience.
        [TestCase(20_000, false)] // Not enough time.
        [TestCase(5_000, false)] // Not enough time to complete beatmap before lenience.
        [TestCase(37_500, true, 2)] // Enough time to complete beatmap after mods are applied.
        public void TestReadyButtonEnablementPeriod(int offsetMs, bool enabled, double rate = 1)
        {
            Room room = null!;
            PlaylistsRoomSubScreen roomScreen = null!;

            AddStep("create room", () =>
            {
                RoomManager.AddRoom(room = new Room
                {
                    Name = @"Test Room",
                    Host = api.LocalUser.Value,
                    Category = RoomCategory.Normal,
                    StartDate = DateTimeOffset.Now,
                    EndDate = DateTimeOffset.Now.AddMilliseconds(offsetMs),
                    Playlist =
                    [
                        new PlaylistItem(importedSet!.Beatmaps[0])
                        {
                            RequiredMods = rate == 1
                                ? []
                                : [new APIMod(new OsuModDoubleTime { SpeedChange = { Value = rate } })]
                        }
                    ]
                });
            });

            AddStep("push screen", () => LoadScreen(roomScreen = new PlaylistsRoomSubScreen(room)));
            AddUntilStep("wait for screen load", () => roomScreen.IsCurrentScreen());
            AddUntilStep("ready button enabled", () => roomScreen.ChildrenOfType<PlaylistsReadyButton>().SingleOrDefault()?.Enabled.Value, () => Is.EqualTo(enabled));
        }
    }
}
