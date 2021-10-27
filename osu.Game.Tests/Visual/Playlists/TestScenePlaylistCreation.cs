// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual.OnlinePlay;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Playlists
{
    public class TestScenePlaylistCreation : OnlinePlayTestScene
    {
        private BeatmapManager manager;
        private RulesetStore rulesets;

        private TestPlaylistsRoomSubScreen match;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, Resources, host, Beatmap.Default));
        }

        [SetUpSteps]
        public void SetupSteps()
        {
            AddStep("set room", () => SelectedRoom.Value = new Room());
            AddStep("ensure has beatmap", () => manager.Import(CreateBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo.BeatmapSet).Wait());
            AddStep("load match", () => LoadScreen(match = new TestPlaylistsRoomSubScreen(SelectedRoom.Value)));
            AddUntilStep("wait for load", () => match.IsCurrentScreen());
        }

        [Test]
        public void TestLoadSimpleMatch()
        {
            setupRoom(room =>
            {
                room.RoomID.Value = 1; // forces room creation.
                room.Name.Value = "my awesome room";
                room.Host.Value = API.LocalUser.Value;
                room.RecentParticipants.Add(room.Host.Value);
                room.EndDate.Value = DateTimeOffset.Now.AddMinutes(5);
                room.Playlist.Add(new PlaylistItem
                {
                    Beatmap = { Value = CreateBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo }
                });

                OnlinePlayDependencies.RoomManager.CreateRoom(SelectedRoom.Value);
            });

            AddStep("start match", () => match.ChildrenOfType<PlaylistsReadyButton>().First().TriggerClick());
            AddUntilStep("player loader loaded", () => Stack.CurrentScreen is PlayerLoader);
        }

        [Test]
        public void TestPlaylistItemSelectedOnCreate()
        {
            setupRoom(room =>
            {
                room.Name.Value = "my awesome room";
                room.Host.Value = API.LocalUser.Value;
                room.Playlist.Add(new PlaylistItem
                {
                    Beatmap = { Value = CreateBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo }
                });

                OnlinePlayDependencies.RoomManager.CreateRoom(SelectedRoom.Value);
            });

            AddStep("move mouse to create button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<PlaylistsRoomSettingsOverlay.CreateRoomButton>().Single());
            });

            AddStep("click", () => InputManager.Click(MouseButton.Left));

            AddAssert("first playlist item selected", () => match.SelectedItem.Value == SelectedRoom.Value.Playlist[0]);
        }

        [Test]
        public void TestBeatmapUpdatedOnReImport()
        {
            BeatmapSetInfo importedSet = null;
            IBeatmap beatmap = null;

            // this step is required to make sure the further imports actually get online IDs.
            // all the playlist logic relies on online ID matching.
            AddStep("remove all matching online IDs", () =>
            {
                beatmap = CreateBeatmap(new OsuRuleset().RulesetInfo);

                var existing = manager.QueryBeatmapSets(s => s.OnlineBeatmapSetID == beatmap.BeatmapInfo.BeatmapSet.OnlineBeatmapSetID).ToList();

                foreach (var s in existing)
                {
                    s.OnlineBeatmapSetID = null;
                    foreach (var b in s.Beatmaps)
                        b.OnlineBeatmapID = null;
                    manager.Update(s);
                }
            });

            AddStep("import altered beatmap", () =>
            {
                beatmap.BeatmapInfo.BaseDifficulty.CircleSize = 1;

                importedSet = manager.Import(beatmap.BeatmapInfo.BeatmapSet).Result.Value;
            });

            setupRoom(room =>
            {
                room.Name.Value = "my awesome room";
                room.Host.Value = API.LocalUser.Value;
                room.Playlist.Add(new PlaylistItem
                {
                    Beatmap = { Value = importedSet.Beatmaps[0] },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo }
                });
            });

            AddStep("create room", () =>
            {
                InputManager.MoveMouseTo(match.ChildrenOfType<PlaylistsRoomSettingsOverlay.CreateRoomButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("match has altered beatmap", () => match.Beatmap.Value.Beatmap.Difficulty.CircleSize == 1);

            AddStep("re-import original beatmap", () => manager.Import(CreateBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo.BeatmapSet).Wait());

            AddAssert("match has original beatmap", () => match.Beatmap.Value.Beatmap.Difficulty.CircleSize != 1);
        }

        private void setupRoom(Action<Room> room)
        {
            AddStep("setup room", () =>
            {
                room(SelectedRoom.Value);

                // if this isn't done the test will crash when a poll kicks in.
                // probably not correct, but works for now.
                OnlinePlayDependencies.RoomManager.CreateRoom(SelectedRoom.Value);
            });
        }

        private class TestPlaylistsRoomSubScreen : PlaylistsRoomSubScreen
        {
            public new Bindable<PlaylistItem> SelectedItem => base.SelectedItem;

            public new Bindable<WorkingBeatmap> Beatmap => base.Beatmap;

            public TestPlaylistsRoomSubScreen(Room room)
                : base(room)
            {
            }
        }
    }
}
