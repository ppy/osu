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
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual.OnlinePlay;
using osu.Game.Users;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Playlists
{
    public class TestScenePlaylistsRoomSubScreen : OnlinePlayTestScene
    {
        private BeatmapManager manager;
        private RulesetStore rulesets;

        private TestPlaylistsRoomSubScreen match;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, Resources, host, Beatmap.Default));

            ((DummyAPIAccess)API).HandleRequest = req =>
            {
                switch (req)
                {
                    case CreateRoomScoreRequest createRoomScoreRequest:
                        createRoomScoreRequest.TriggerSuccess(new APIScoreToken { ID = 1 });
                        return true;
                }

                return false;
            };
        }

        [SetUpSteps]
        public void SetupSteps()
        {
            AddStep("set room", () => SelectedRoom.Value = new Room());
            AddStep("ensure has beatmap", () => manager.Import(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo.BeatmapSet).Wait());
            AddStep("load match", () => LoadScreen(match = new TestPlaylistsRoomSubScreen(SelectedRoom.Value)));
            AddUntilStep("wait for load", () => match.IsCurrentScreen());
        }

        [Test]
        public void TestLoadSimpleMatch()
        {
            AddStep("set room properties", () =>
            {
                SelectedRoom.Value.RoomID.Value = 1;
                SelectedRoom.Value.Name.Value = "my awesome room";
                SelectedRoom.Value.Host.Value = new User { Id = 2, Username = "peppy" };
                SelectedRoom.Value.RecentParticipants.Add(SelectedRoom.Value.Host.Value);
                SelectedRoom.Value.EndDate.Value = DateTimeOffset.Now.AddMinutes(5);
                SelectedRoom.Value.Playlist.Add(new PlaylistItem
                {
                    Beatmap = { Value = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo }
                });
            });

            AddStep("start match", () => match.ChildrenOfType<PlaylistsReadyButton>().First().Click());
            AddUntilStep("player loader loaded", () => Stack.CurrentScreen is PlayerLoader);
        }

        [Test]
        public void TestPlaylistItemSelectedOnCreate()
        {
            AddStep("set room properties", () =>
            {
                SelectedRoom.Value.Name.Value = "my awesome room";
                SelectedRoom.Value.Host.Value = new User { Id = 2, Username = "peppy" };
                SelectedRoom.Value.Playlist.Add(new PlaylistItem
                {
                    Beatmap = { Value = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo }
                });
            });

            AddStep("move mouse to create button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<PlaylistsMatchSettingsOverlay.CreateRoomButton>().Single());
            });

            AddStep("click", () => InputManager.Click(MouseButton.Left));

            AddAssert("first playlist item selected", () => match.SelectedItem.Value == SelectedRoom.Value.Playlist[0]);
        }

        [Test]
        public void TestBeatmapUpdatedOnReImport()
        {
            BeatmapSetInfo importedSet = null;
            TestBeatmap beatmap = null;

            // this step is required to make sure the further imports actually get online IDs.
            // all the playlist logic relies on online ID matching.
            AddStep("remove all matching online IDs", () =>
            {
                beatmap = new TestBeatmap(new OsuRuleset().RulesetInfo);

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

                importedSet = manager.Import(beatmap.BeatmapInfo.BeatmapSet).Result;
            });

            AddStep("load room", () =>
            {
                SelectedRoom.Value.Name.Value = "my awesome room";
                SelectedRoom.Value.Host.Value = new User { Id = 2, Username = "peppy" };
                SelectedRoom.Value.Playlist.Add(new PlaylistItem
                {
                    Beatmap = { Value = importedSet.Beatmaps[0] },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo }
                });
            });

            AddStep("create room", () =>
            {
                InputManager.MoveMouseTo(match.ChildrenOfType<PlaylistsMatchSettingsOverlay.CreateRoomButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("match has altered beatmap", () => match.Beatmap.Value.Beatmap.BeatmapInfo.BaseDifficulty.CircleSize == 1);

            AddStep("re-import original beatmap", () => manager.Import(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo.BeatmapSet).Wait());

            AddAssert("match has original beatmap", () => match.Beatmap.Value.Beatmap.BeatmapInfo.BaseDifficulty.CircleSize != 1);
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
