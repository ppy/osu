// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Playlists
{
    public class TestScenePlaylistsMatchSettingsOverlay : OnlinePlayTestScene
    {
        protected new TestRoomManager RoomManager => (TestRoomManager)base.RoomManager;

        private TestRoomSettings settings;

        protected override RoomTestDependencies CreateRoomDependencies() => new TestDependencies();

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            SelectedRoom.Value = new Room();

            Child = settings = new TestRoomSettings
            {
                RelativeSizeAxes = Axes.Both,
                State = { Value = Visibility.Visible }
            };
        });

        [Test]
        public void TestButtonEnabledOnlyWithNameAndBeatmap()
        {
            AddStep("clear name and beatmap", () =>
            {
                SelectedRoom.Value.Name.Value = "";
                SelectedRoom.Value.Playlist.Clear();
            });

            AddAssert("button disabled", () => !settings.ApplyButton.Enabled.Value);

            AddStep("set name", () => SelectedRoom.Value.Name.Value = "Room name");
            AddAssert("button disabled", () => !settings.ApplyButton.Enabled.Value);

            AddStep("set beatmap", () => SelectedRoom.Value.Playlist.Add(new PlaylistItem { Beatmap = { Value = CreateBeatmap(Ruleset.Value).BeatmapInfo } }));
            AddAssert("button enabled", () => settings.ApplyButton.Enabled.Value);

            AddStep("clear name", () => SelectedRoom.Value.Name.Value = "");
            AddAssert("button disabled", () => !settings.ApplyButton.Enabled.Value);
        }

        [Test]
        public void TestCorrectSettingsApplied()
        {
            const string expected_name = "expected name";
            TimeSpan expectedDuration = TimeSpan.FromMinutes(15);

            Room createdRoom = null;

            AddStep("setup", () =>
            {
                settings.NameField.Current.Value = expected_name;
                settings.DurationField.Current.Value = expectedDuration;
                SelectedRoom.Value.Playlist.Add(new PlaylistItem { Beatmap = { Value = CreateBeatmap(Ruleset.Value).BeatmapInfo } });

                RoomManager.CreateRequested = r =>
                {
                    createdRoom = r;
                    return true;
                };
            });

            AddStep("create room", () => settings.ApplyButton.Action.Invoke());
            AddAssert("has correct name", () => createdRoom.Name.Value == expected_name);
            AddAssert("has correct duration", () => createdRoom.Duration.Value == expectedDuration);
        }

        [Test]
        public void TestCreationFailureDisplaysError()
        {
            bool fail;

            AddStep("setup", () =>
            {
                SelectedRoom.Value.Name.Value = "Test Room";
                SelectedRoom.Value.Playlist.Add(new PlaylistItem { Beatmap = { Value = CreateBeatmap(Ruleset.Value).BeatmapInfo } });

                fail = true;
                RoomManager.CreateRequested = _ => !fail;
            });
            AddAssert("error not displayed", () => !settings.ErrorText.IsPresent);

            AddStep("create room", () => settings.ApplyButton.Action.Invoke());
            AddAssert("error displayed", () => settings.ErrorText.IsPresent);
            AddAssert("error has correct text", () => settings.ErrorText.Text == TestRoomManager.FAILED_TEXT);

            AddStep("create room no fail", () =>
            {
                fail = false;
                settings.ApplyButton.Action.Invoke();
            });

            AddUntilStep("error not displayed", () => !settings.ErrorText.IsPresent);
        }

        private class TestRoomSettings : PlaylistsMatchSettingsOverlay
        {
            public TriangleButton ApplyButton => ((MatchSettings)Settings).ApplyButton;

            public OsuTextBox NameField => ((MatchSettings)Settings).NameField;
            public OsuDropdown<TimeSpan> DurationField => ((MatchSettings)Settings).DurationField;

            public OsuSpriteText ErrorText => ((MatchSettings)Settings).ErrorText;
        }

        private class TestDependencies : RoomTestDependencies
        {
            protected override IRoomManager CreateRoomManager() => new TestRoomManager();
        }

        protected class TestRoomManager : IRoomManager
        {
            public const string FAILED_TEXT = "failed";

            public Func<Room, bool> CreateRequested;

            public event Action RoomsUpdated
            {
                add { }
                remove { }
            }

            public IBindable<bool> InitialRoomsReceived { get; } = new Bindable<bool>(true);

            public IBindableList<Room> Rooms => null;

            public void CreateRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null)
            {
                if (CreateRequested == null)
                    return;

                if (!CreateRequested.Invoke(room))
                    onError?.Invoke(FAILED_TEXT);
                else
                    onSuccess?.Invoke(room);
            }

            public void JoinRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null) => throw new NotImplementedException();

            public void PartRoom() => throw new NotImplementedException();
        }
    }
}
