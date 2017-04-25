// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Testing;
using osu.Game.Screens.Tournament;
using osu.Game.Screens.Tournament.Teams;
using osu.Game.Users;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseDrawings : TestCase
    {
        public override string Description => "Tournament drawings";

        public override void Reset()
        {
            base.Reset();

            Add(new Drawings
            {
                TeamList = new TestTeamList(),
            });
        }

        private class TestTeamList : ITeamList
        {
            public IEnumerable<Country> Teams { get; } = new[]
            {
                new Country
                {
                    FlagName = "GB",
                    FullName = "United Kingdom",
                    Acronym = "UK"
                },
                new Country
                {
                    FlagName = "FR",
                    FullName = "France",
                    Acronym = "FRA"
                },
                new Country
                {
                    FlagName = "CN",
                    FullName = "China",
                    Acronym = "CHN"
                },
                new Country
                {
                    FlagName = "AU",
                    FullName = "Australia",
                    Acronym = "AUS"
                },
                new Country
                {
                    FlagName = "JP",
                    FullName = "Japan",
                    Acronym = "JPN"
                },
                new Country
                {
                    FlagName = "RO",
                    FullName = "Romania",
                    Acronym = "ROM"
                },
                new Country
                {
                    FlagName = "IT",
                    FullName = "Italy",
                    Acronym = "PIZZA"
                },
                new Country
                {
                    FlagName = "VE",
                    FullName = "Venezuela",
                    Acronym = "VNZ"
                },
                new Country
                {
                    FlagName = "US",
                    FullName = "United States of America",
                    Acronym = "USA"
                },
            };
        }
    }
}
