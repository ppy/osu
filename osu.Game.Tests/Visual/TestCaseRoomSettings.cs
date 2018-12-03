// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing.Input;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Components;
using osuTK.Input;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseRoomSettings : ManualInputManagerTestCase
    {
        private readonly Room room;
        private readonly TestRoomSettingsOverlay overlay;

        public TestCaseRoomSettings()
        {
            room = new Room
            {
                Name = { Value = "One Testing Room" },
                Availability = { Value = RoomAvailability.Public },
                Type = { Value = new GameTypeTeamVersus() },
                MaxParticipants = { Value = 10 },
            };

            Add(overlay = new TestRoomSettingsOverlay(room)
            {
                RelativeSizeAxes = Axes.Both,
                Height = 0.75f,
            });

            AddStep(@"show", overlay.Show);
            assertAll();
            AddStep(@"set name", () => overlay.CurrentName = @"Two Testing Room");
            AddStep(@"set max", () => overlay.CurrentMaxParticipants = null);
            AddStep(@"set availability", () => overlay.CurrentAvailability = RoomAvailability.InviteOnly);
            AddStep(@"set type", () => overlay.CurrentType = new GameTypeTagTeam());
            apply();
            assertAll();
            AddStep(@"show", overlay.Show);
            AddStep(@"set room name", () => room.Name.Value = @"Room Changed Name!");
            AddStep(@"set room availability", () => room.Availability.Value = RoomAvailability.Public);
            AddStep(@"set room type", () => room.Type.Value = new GameTypeTag());
            AddStep(@"set room max", () => room.MaxParticipants.Value = 100);
            assertAll();
            AddStep(@"set name", () => overlay.CurrentName = @"Unsaved Testing Room");
            AddStep(@"set max", () => overlay.CurrentMaxParticipants = 20);
            AddStep(@"set availability", () => overlay.CurrentAvailability = RoomAvailability.FriendsOnly);
            AddStep(@"set type", () => overlay.CurrentType = new GameTypeVersus());
            AddStep(@"hide", overlay.Hide);
            AddWaitStep(5);
            AddStep(@"show", overlay.Show);
            assertAll();
            AddStep(@"hide", overlay.Hide);
        }

        private void apply()
        {
            AddStep(@"apply", () =>
            {
                overlay.ClickApplyButton(InputManager);
            });
        }

        private void assertAll()
        {
            AddAssert(@"name == room name", () => overlay.CurrentName == room.Name.Value);
            AddAssert(@"max == room max", () => overlay.CurrentMaxParticipants == room.MaxParticipants.Value);
            AddAssert(@"availability == room availability", () => overlay.CurrentAvailability == room.Availability.Value);
            AddAssert(@"type == room type", () => Equals(overlay.CurrentType, room.Type.Value));
        }

        private class TestRoomSettingsOverlay : RoomSettingsOverlay
        {
            public string CurrentName
            {
                get => NameField.Text;
                set => NameField.Text = value;
            }

            public int? CurrentMaxParticipants
            {
                get
                {
                    if (int.TryParse(MaxParticipantsField.Text, out int max))
                        return max;

                    return null;
                }
                set => MaxParticipantsField.Text = value?.ToString();
            }

            public RoomAvailability CurrentAvailability
            {
                get => AvailabilityPicker.Current.Value;
                set => AvailabilityPicker.Current.Value = value;
            }

            public GameType CurrentType
            {
                get => TypePicker.Current.Value;
                set => TypePicker.Current.Value = value;
            }

            public TestRoomSettingsOverlay(Room room) : base(room)
            {
            }

            public void ClickApplyButton(ManualInputManager inputManager)
            {
                inputManager.MoveMouseTo(ApplyButton);
                inputManager.Click(MouseButton.Left);
            }
        }
    }
}
