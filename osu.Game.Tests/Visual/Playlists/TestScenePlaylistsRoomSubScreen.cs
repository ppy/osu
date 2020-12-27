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
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Tests.Beatmaps;
using osu.Game.Users;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Playlists
{
    public class TestScenePlaylistsRoomSubScreen : RoomTestScene
    {
        protected override bool UseOnlineAPI => true;

        [Cached(typeof(IRoomManager))]
        private readonly TestRoomManager roomManager = new TestRoomManager();

        private BeatmapManager manager;
        private RulesetStore rulesets;

        private TestPlaylistsRoomSubScreen match;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, host, Beatmap.Default));

            manager.Import(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo.BeatmapSet).Wait();
        }

        [SetUpSteps]
        public void SetupSteps()
        {
            AddStep("load match", () => LoadScreen(match = new TestPlaylistsRoomSubScreen(Room)));
            AddUntilStep("wait for load", () => match.IsCurrentScreen());
        }

        [Test]
        public void TestLoadSimpleMatch()
        {
            AddStep("set room properties", () =>
            {
                Room.RoomID.Value = 1;
                Room.Name.Value = "my awesome room";
                Room.Host.Value = new User { Id = 2, Username = "peppy" };
                Room.RecentParticipants.Add(Room.Host.Value);
                Room.Playlist.Add(new PlaylistItem
                {
                    Beatmap = { Value = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo }
                });
            });
        }

        [Test]
        public void TestPlaylistItemSelectedOnCreate()
        {
            AddStep("set room properties", () =>
            {
                Room.Name.Value = "my awesome room";
                Room.Host.Value = new User { Id = 2, Username = "peppy" };
                Room.Playlist.Add(new PlaylistItem
                {
                    Beatmap = { Value = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo }
                });
            });

            AddStep("move mouse to create button", () =>
            {
                var footer = match.ChildrenOfType<Footer>().Single();
                InputManager.MoveMouseTo(footer.ChildrenOfType<OsuButton>().Single());
            });

            AddStep("click", () => InputManager.Click(MouseButton.Left));

            AddAssert("first playlist item selected", () => match.SelectedItem.Value == Room.Playlist[0]);
        }

        [Test]
        public void TestBeatmapUpdatedOnReImport()
        {
            BeatmapSetInfo importedSet = null;

            AddStep("import altered beatmap", () =>
            {
                var beatmap = new TestBeatmap(new OsuRuleset().RulesetInfo);
                beatmap.BeatmapInfo.BaseDifficulty.CircleSize = 1;

                importedSet = manager.Import(beatmap.BeatmapInfo.BeatmapSet).Result;
            });

            AddStep("load room", () =>
            {
                Room.Name.Value = "my awesome room";
                Room.Host.Value = new User { Id = 2, Username = "peppy" };
                Room.Playlist.Add(new PlaylistItem
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

        private class TestRoomManager : IRoomManager
        {
            public event Action RoomsUpdated
            {
                add => throw new NotImplementedException();
                remove => throw new NotImplementedException();
            }

            public IBindable<bool> InitialRoomsReceived { get; } = new Bindable<bool>(true);

            public IBindableList<Room> Rooms { get; } = new BindableList<Room>();

            public void CreateRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null)
            {
                room.RoomID.Value = 1;
                onSuccess?.Invoke(room);
            }

            public void JoinRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null) => onSuccess?.Invoke(room);

            public void PartRoom()
            {
            }
        }
    }
}
