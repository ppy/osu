// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Playlists
{
    public partial class TestScenePlaylistsMatchSettingsOverlay : OnlinePlayTestScene
    {
        private TestRoomSettings settings = null!;
        private Room room = null!;
        private Func<Room, string?>? handleRequest;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("setup api", () =>
            {
                handleRequest = null;
                ((DummyAPIAccess)API).HandleRequest = req =>
                {
                    if (req is not CreateRoomRequest createReq || handleRequest == null)
                        return false;

                    if (handleRequest(createReq.Room) is string errorText)
                        createReq.TriggerFailure(new APIException(errorText, null));
                    else
                    {
                        var createdRoom = new APICreatedRoom();
                        createdRoom.CopyFrom(createReq.Room);
                        createReq.TriggerSuccess(createdRoom);
                    }

                    return true;
                };
            });

            AddStep("create overlay", () =>
            {
                Child = settings = new TestRoomSettings(room = new Room())
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
                room.Name = "";
                room.Playlist = [];
            });

            AddAssert("button disabled", () => !settings.ApplyButton.Enabled.Value);

            AddStep("set name", () => room.Name = "Room name");
            AddAssert("button disabled", () => !settings.ApplyButton.Enabled.Value);

            AddStep("set beatmap", () => room.Playlist = [new PlaylistItem(CreateBeatmap(Ruleset.Value).BeatmapInfo)]);
            AddAssert("button enabled", () => settings.ApplyButton.Enabled.Value);

            AddStep("clear name", () => room.Name = "");
            AddAssert("button disabled", () => !settings.ApplyButton.Enabled.Value);
        }

        [Test]
        public void TestCorrectSettingsApplied()
        {
            const string expected_name = "expected name";
            TimeSpan expectedDuration = TimeSpan.FromMinutes(15);

            Room createdRoom = null!;

            AddStep("setup", () =>
            {
                settings.NameField.Current.Value = expected_name;
                settings.DurationField.Current.Value = expectedDuration;
                room.Playlist = [new PlaylistItem(CreateBeatmap(Ruleset.Value).BeatmapInfo)];

                handleRequest = r =>
                {
                    createdRoom = r;
                    return null;
                };
            });

            AddStep("create room", () => settings.ApplyButton.Action.Invoke());
            AddAssert("has correct name", () => createdRoom.Name == expected_name);
            AddAssert("has correct duration", () => createdRoom.Duration == expectedDuration);
        }

        [Test]
        public void TestInvalidBeatmapError()
        {
            const string not_found_prefix = "beatmaps not found:";

            string errorMessage = null!;

            AddStep("setup", () =>
            {
                var beatmap = CreateBeatmap(Ruleset.Value).BeatmapInfo;

                room.Name = "Test Room";
                room.Playlist = [new PlaylistItem(beatmap)];

                errorMessage = $"{not_found_prefix} {beatmap.OnlineID}";

                handleRequest = _ => errorMessage;
            });

            AddAssert("error not displayed", () => !settings.ErrorText.IsPresent);
            AddAssert("playlist item valid", () => room.Playlist[0].Valid.Value);

            AddStep("create room", () => settings.ApplyButton.Action.Invoke());

            AddAssert("error displayed", () => settings.ErrorText.IsPresent);
            AddAssert("error has custom text", () => settings.ErrorText.Text != errorMessage);
            AddAssert("playlist item marked invalid", () => !room.Playlist[0].Valid.Value);
        }

        [Test]
        public void TestCreationFailureDisplaysError()
        {
            const string error_message = "failed";

            string failText = error_message;

            AddStep("setup", () =>
            {
                room.Name = "Test Room";
                room.Playlist = [new PlaylistItem(CreateBeatmap(Ruleset.Value).BeatmapInfo)];

                handleRequest = _ => failText;
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
    }
}
