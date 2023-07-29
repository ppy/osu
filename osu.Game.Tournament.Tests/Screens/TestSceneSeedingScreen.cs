// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Ladder.Components;
using osu.Game.Tournament.Screens.TeamIntro;

namespace osu.Game.Tournament.Tests.Screens
{
    public partial class TestSceneSeedingScreen : TournamentScreenTestScene
    {
        [Cached]
        private readonly LadderInfo ladder = new LadderInfo
        {
            Teams =
            {
                new TournamentTeam
                {
                    FullName = { Value = @"Japan" },
                    Acronym = { Value = "JPN" },
                    SeedingResults =
                    {
                        new SeedingResult
                        {
                            // Mod intentionally left blank.
                            Seed = { Value = 4 }
                        },
                        new SeedingResult
                        {
                            Mod = { Value = "DT" },
                            Seed = { Value = 8 }
                        }
                    }
                }
            }
        };

        [Test]
        public void TestBasic()
        {
            AddStep("create seeding screen", () => Add(new SeedingScreen
            {
                FillMode = FillMode.Fit,
                FillAspectRatio = 16 / 9f
            }));

            AddStep("set team to Japan", () => this.ChildrenOfType<SettingsTeamDropdown>().Single().Current.Value = ladder.Teams.Single());
        }
    }
}
