// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Game.Screens.Tournament;
using osu.Game.Screens.Tournament.Teams;

namespace osu.Game.Tests.Visual.Tournament
{
    [Description("for tournament use")]
    public class TestCaseDrawings : ScreenTestCase
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            LoadScreen(new Drawings
            {
                TeamList = new TestTeamList(),
            });
        }

        private class TestTeamList : ITeamList
        {
            public IEnumerable<DrawingsTeam> Teams { get; } = new[]
            {
                new DrawingsTeam
                {
                    FlagName = "GB",
                    FullName = "United Kingdom",
                    Acronym = "UK"
                },
                new DrawingsTeam
                {
                    FlagName = "FR",
                    FullName = "France",
                    Acronym = "FRA"
                },
                new DrawingsTeam
                {
                    FlagName = "CN",
                    FullName = "China",
                    Acronym = "CHN"
                },
                new DrawingsTeam
                {
                    FlagName = "AU",
                    FullName = "Australia",
                    Acronym = "AUS"
                },
                new DrawingsTeam
                {
                    FlagName = "JP",
                    FullName = "Japan",
                    Acronym = "JPN"
                },
                new DrawingsTeam
                {
                    FlagName = "RO",
                    FullName = "Romania",
                    Acronym = "ROM"
                },
                new DrawingsTeam
                {
                    FlagName = "IT",
                    FullName = "Italy",
                    Acronym = "PIZZA"
                },
                new DrawingsTeam
                {
                    FlagName = "VE",
                    FullName = "Venezuela",
                    Acronym = "VNZ"
                },
                new DrawingsTeam
                {
                    FlagName = "US",
                    FullName = "United States of America",
                    Acronym = "USA"
                },
            };
        }
    }
}
