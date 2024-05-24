// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Playlists
{
    public partial class TestScenePlaylistsMatchSettingsOverlay : OnlinePlayTestScene
    {
        protected new TestRoomManager RoomManager => (TestRoomManager)base.RoomManager;

        private TestRoomSettings settings;

        protected override OnlinePlayTestSceneDependencies CreateOnlinePlayDependencies() => new TestDependencies();

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("create overlay", () =>
            {
                SelectedRoom.Value = new Room();

                Child = settings = new TestRoomSettings(SelectedRoom.Value)
                {
                    RelativeSizeAxes = Axes.Both,
                    State = { Value = Visibility.Visible }
                };
            });
        }

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

            AddStep("set beatmap", () => SelectedRoom.Value.Playlist.Add(new PlaylistItem(CreateBeatmap(Ruleset.Value).BeatmapInfo)));
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
                SelectedRoom.Value.Playlist.Add(new PlaylistItem(CreateBeatmap(Ruleset.Value).BeatmapInfo));

                RoomManager.CreateRequested = r =>
                {
                    createdRoom = r;
                    return string.Empty;
                };
            });

            AddStep("create room", () => settings.ApplyButton.Action.Invoke());
            AddAssert("has correct name", () => createdRoom.Name.Value == expected_name);
            AddAssert("has correct duration", () => createdRoom.Duration.Value == expectedDuration);
        }

        [Test]
        public void TestInvalidBeatmapError()
        {
            const string not_found_prefix = "beatmaps not found:";

            string errorMessage = null;

            AddStep("setup", () =>
            {
                var beatmap = CreateBeatmap(Ruleset.Value).BeatmapInfo;

                SelectedRoom.Value.Name.Value = "Test Room";
                SelectedRoom.Value.Playlist.Add(new PlaylistItem(beatmap));

                errorMessage = $"{not_found_prefix} {beatmap.OnlineID}";

                RoomManager.CreateRequested = _ => errorMessage;
            });

            AddAssert("error not displayed", () => !settings.ErrorText.IsPresent);
            AddAssert("playlist item valid", () => SelectedRoom.Value.Playlist[0].Valid.Value);

            AddStep("create room", () => settings.ApplyButton.Action.Invoke());

            AddAssert("error displayed", () => settings.ErrorText.IsPresent);
            AddAssert("error has custom text", () => settings.ErrorText.Text != errorMessage);
            AddAssert("playlist item marked invalid", () => !SelectedRoom.Value.Playlist[0].Valid.Value);
        }

        [Test]
        public void TestCreationFailureDisplaysError()
        {
            const string error_message = "failed";

            string failText = error_message;

            AddStep("setup", () =>
            {
                SelectedRoom.Value.Name.Value = "Test Room";
                SelectedRoom.Value.Playlist.Add(new PlaylistItem(CreateBeatmap(Ruleset.Value).BeatmapInfo));

                RoomManager.CreateRequested = _ => failText;
            });
            AddAssert("error not displayed", () => !settings.ErrorText.IsPresent);

            AddStep("create room", () => settings.ApplyButton.Action.Invoke());
            AddAssert("error displayed", () => settings.ErrorText.IsPresent);
            AddAssert("error has correct text", () => settings.ErrorText.Text == error_message);

            AddStep("create room no fail", () =>
            {
                failText = string.Empty;
                settings.ApplyButton.Action.Invoke();
            });

            AddUntilStep("error not displayed", () => !settings.ErrorText.IsPresent);
        }

        private partial class TestRoomSettings : PlaylistsRoomSettingsOverlay
        {
            public RoundedButton ApplyButton => ((MatchSettings)Settings).ApplyButton;

            public OsuTextBox NameField => ((MatchSettings)Settings).NameField;
            public OsuDropdown<TimeSpan> DurationField => ((MatchSettings)Settings).DurationField;

            public OsuSpriteText ErrorText => ((MatchSettings)Settings).ErrorText;

            public TestRoomSettings(Room room)
                : base(room)
            {
            }
        }

        private class TestDependencies : OnlinePlayTestSceneDependencies
        {
            protected override IRoomManager CreateRoomManager() => new TestRoomManager();
        }

        protected class TestRoomManager : IRoomManager
        {
            public Func<Room, string> CreateRequested;

            public event Action RoomsUpdated
            {
                add { }
                remove { }
            }

            public IBindable<bool> InitialRoomsReceived { get; } = new Bindable<bool>(true);

            public IBindableList<Room> Rooms => null!;

            public void AddOrUpdateRoom(Room room) => throw new NotImplementedException();

            public void RemoveRoom(Room room) => throw new NotImplementedException();

            public void ClearRooms() => throw new NotImplementedException();

            public void CreateRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null)
            {
                if (CreateRequested == null)
                    return;

                string error = CreateRequested.Invoke(room);

                if (!string.IsNullOrEmpty(error))
                    onError?.Invoke(error);
                else
                    onSuccess?.Invoke(room);
            }

            public void JoinRoom(Room room, string password, Action<Room> onSuccess = null, Action<string> onError = null) => throw new NotImplementedException();

            public void PartRoom() => throw new NotImplementedException();
        }
    }
}
