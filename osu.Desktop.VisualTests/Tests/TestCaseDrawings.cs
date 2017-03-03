using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Framework.Screens.Testing;
using osu.Game.Screens.Tournament;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            Add(new Drawings(new TestTeamList()));
        }

        class TestTeamList : ITeamList
        {
            public IEnumerable<Team> Teams
            {
                get
                {
                    return new Team[]
                    {
                        new Team()
                        {
                            FlagName = "GB",
                            FullName = "United Kingdom",
                            Acronym = "UK"
                        },
                        new Team()
                        {
                            FlagName = "FR",
                            FullName = "France",
                            Acronym = "FRA"
                        },
                        new Team()
                        {
                            FlagName = "CN",
                            FullName = "China",
                            Acronym = "CHN"
                        },
                        new Team()
                        {
                            FlagName = "AU",
                            FullName = "Australia",
                            Acronym = "AUS"
                        },
                        new Team()
                        {
                            FlagName = "JP",
                            FullName = "Japan",
                            Acronym = "JPN"
                        },
                        new Team()
                        {
                            FlagName = "RO",
                            FullName = "Romania",
                        },
                        new Team()
                        {
                            FlagName = "IT",
                            FullName = "Italy",
                        },
                        new Team()
                        {
                            FlagName = "VE",
                            FullName = "Venezuela",
                            Acronym = "VNZ"
                        },
                        new Team()
                        {
                            FlagName = "US",
                            FullName = "United States of America",
                            Acronym = "USA"
                        }
                    };
                }
            }
        }
    }
}
