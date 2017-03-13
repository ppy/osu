// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Screens.Testing;
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
            public IEnumerable<Region> Teams { get; } = new[]
            {
                new Region
                {
                    FlagName = "GB",
                    FullName = "United Kingdom",
                    Acronym = "UK"
                },
                new Region
                {
                    FlagName = "FR",
                    FullName = "France",
                    Acronym = "FRA"
                },
                new Region
                {
                    FlagName = "CN",
                    FullName = "China",
                    Acronym = "CHN"
                },
                new Region
                {
                    FlagName = "AU",
                    FullName = "Australia",
                    Acronym = "AUS"
                },
                new Region
                {
                    FlagName = "JP",
                    FullName = "Japan",
                    Acronym = "JPN"
                },
                new Region
                {
                    FlagName = "RO",
                    FullName = "Romania",
                    Acronym = "ROM"
                },
                new Region
                {
                    FlagName = "IT",
                    FullName = "Italy",
                    Acronym = "PIZZA"
                },
                new Region
                {
                    FlagName = "VE",
                    FullName = "Venezuela",
                    Acronym = "VNZ"
                },
                new Region
                {
                    FlagName = "US",
                    FullName = "United States of America",
                    Acronym = "USA"
                },
            };
        }
    }
}
