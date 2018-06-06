// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Screens.Match;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseRoomSettings : OsuTestCase
    {
        public TestCaseRoomSettings()
        {
            Room room = new Room();

            RoomSettingsOverlay overlay;
            Add(overlay = new RoomSettingsOverlay(room)
            {
                RelativeSizeAxes = Axes.Both,
                Height = 0.75f,
            });

            AddStep(@"toggle", overlay.ToggleVisibility);
        }
    }
}
