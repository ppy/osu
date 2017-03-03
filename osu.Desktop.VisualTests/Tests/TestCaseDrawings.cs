// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Framework.Screens.Testing;
using osu.Game.Screens.Tournament;
using osu.Game.Screens.Tournament.Teams;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseDrawings : TestCase
    {
        public override string Name => @"Drawings";
        public override string Description => "Tournament drawings";

        [BackgroundDependencyLoader]
        private void load(Storage storage)
        {
        }

        public override void Reset()
        {
            base.Reset();

            Add(new Drawings
            {
                TeamList = new TestTeamList(),
            });
        }

        class TestTeamList : ITeamList
        {
            public IEnumerable<Team> Teams { get; } = new[]
            {
                new Team
                {
                    FlagName = "GB",
                    FullName = "United Kingdom",
                    Acronym = "UK"
                },
                new Team
                {
                    FlagName = "FR",
                    FullName = "France",
                    Acronym = "FRA"
                },
                new Team
                {
                    FlagName = "CN",
                    FullName = "China",
                    Acronym = "CHN"
                },
                new Team
                {
                    FlagName = "AU",
                    FullName = "Australia",
                    Acronym = "AUS"
                },
                new Team
                {
                    FlagName = "JP",
                    FullName = "Japan",
                    Acronym = "JPN"
                },
                new Team
                {
                    FlagName = "RO",
                    FullName = "Romania",
                    Acronym = "ROM"
                },
                new Team
                {
                    FlagName = "IT",
                    FullName = "Italy",
                    Acronym = "PIZZA"
                },
                new Team
                {
                    FlagName = "VE",
                    FullName = "Venezuela",
                    Acronym = "VNZ"
                },
                new Team
                {
                    FlagName = "US",
                    FullName = "United States of America",
                    Acronym = "USA"
                },
            };
        }
    }
}
