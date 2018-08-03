// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Screens.Match;

namespace osu.Game.Tests.Visual
{
    public class TestCaseMatchSettings : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Settings),
        };

        public TestCaseMatchSettings()
        {
            Settings settings

            Add(settings = new Settings
            {
                RelativeSizeAxes = Axes.Both,
            });

            AddStep(@"change room name", () => settings.RoomName.Value = @"A Different Awesome Room");
            AddStep(@"change max participants", () => settings.MaxParticipants.Value = 16);
            AddStep(@"null max participants", () => settings.MaxParticipants.Value = null);
            AddStep(@"change room visibility", () => settings.RoomAvailability.Value = RoomAvailability.FriendsOnly);
            AddStep(@"change password", () => settings.Password.Value = @"HaxxorPassword");
            AddStep(@"change game type", () => settings.GameType.Value = GameType.TeamVersus);
        }
    }
}
