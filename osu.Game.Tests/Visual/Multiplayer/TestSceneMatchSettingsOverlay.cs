// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi;
using osu.Game.Screens.Multi.Match.Components;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMatchSettingsOverlay : MultiplayerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(MatchSettingsOverlay)
        };

        [Cached(Type = typeof(IRoomManager))]
        private TestRoomManager roomManager = new TestRoomManager();

        private TestRoomSettings settings;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Room = new Room();

            settings = new TestRoomSettings
            {
                RelativeSizeAxes = Axes.Both,
                State = { Value = Visibility.Visible }
            };

            Child = settings;
        });

        [Test]
        public void TestButtonEnabledOnlyWithNameAndBeatmap()
        {
            AddStep("clear name and beatmap", () =>
            {
                Room.Name.Value = "";
                Room.Playlist.Clear();
            });

            AddAssert("button disabled", () => !settings.ApplyButton.Enabled.Value);

            AddStep("set name", () => Room.Name.Value = "Room name");
            AddAssert("button disabled", () => !settings.ApplyButton.Enabled.Value);

            AddStep("set beatmap", () => Room.Playlist.Add(new PlaylistItem { Beatmap = CreateBeatmap(Ruleset.Value).BeatmapInfo }));
            AddAssert("button enabled", () => settings.ApplyButton.Enabled.Value);

            AddStep("clear name", () => Room.Name.Value = "");
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

                roomManager.CreateRequested = r =>
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
                fail = true;
                roomManager.CreateRequested = _ => !fail;
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

        private class TestRoomSettings : MatchSettingsOverlay
        {
            public TriangleButton ApplyButton => Settings.ApplyButton;

            public OsuTextBox NameField => Settings.NameField;
            public OsuDropdown<TimeSpan> DurationField => Settings.DurationField;

            public OsuSpriteText ErrorText => Settings.ErrorText;
        }

        private class TestRoomManager : IRoomManager
        {
            public const string FAILED_TEXT = "failed";

            public Func<Room, bool> CreateRequested;

            public event Action RoomsUpdated
            {
                add { }
                remove { }
            }

            public IBindableList<Room> Rooms { get; } = null;

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
