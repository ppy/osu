// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Screens.Ladder.Components;

namespace osu.Game.Tournament.Tests
{
    public class TestCaseMatchPairings : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(MatchPairing),
            typeof(DrawableMatchPairing),
            typeof(DrawableMatchTeam),
            typeof(DrawableTournamentTeam),
        };

        public TestCaseMatchPairings()
        {
            var pairing1 = new MatchPairing(
                new TournamentTeam { FlagName = "AU", FullName = "Australia", },
                new TournamentTeam { FlagName = "JP", FullName = "Japan", Acronym = "JPN" })
            {
                Team1Score = { Value = 8 },
                Team2Score = { Value = 6 },
            };

            var pairing2 = new MatchPairing(
                new TournamentTeam
                {
                    FlagName = "RO",
                    FullName = "Romania",
                }
            );

            Child = new FillFlowContainer
            {
                Children = new Drawable[]
                {
                    new DrawableMatchPairing(pairing1),
                    new DrawableMatchPairing(pairing2),
                    new DrawableMatchPairing(new MatchPairing())
                }
            };

            AddStep("mark complete", () => pairing1.Completed.Value = true);
            AddRepeatStep("change scores", () => pairing1.Team2Score.Value++, 5);
            AddStep("mark complete", () => pairing1.Completed.Value = true);

            AddStep("add new team", () => pairing2.Team2.Value =
                new TournamentTeam { FlagName = "PT", FullName = "Portugal" }
            );
        }
    }
}
