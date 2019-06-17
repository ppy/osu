// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Screens.Ladder.Components;

namespace osu.Game.Tournament.Tests
{
    public class TestSceneMatchPairings : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(MatchPairing),
            typeof(DrawableMatchPairing),
            typeof(DrawableMatchTeam),
            typeof(DrawableTournamentTeam),
        };

        public TestSceneMatchPairings()
        {
            Container<DrawableMatchPairing> level1;
            Container<DrawableMatchPairing> level2;

            var pairing1 = new MatchPairing(
                new TournamentTeam { FlagName = { Value = "AU" }, FullName = { Value = "Australia" }, },
                new TournamentTeam { FlagName = { Value = "JP" }, FullName = { Value = "Japan" }, Acronym = { Value = "JPN" } })
            {
                Team1Score = { Value = 4 },
                Team2Score = { Value = 1 },
            };

            var pairing2 = new MatchPairing(
                new TournamentTeam
                {
                    FlagName = { Value = "RO" },
                    FullName = { Value = "Romania" },
                }
            );

            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Children = new Drawable[]
                {
                    level1 = new FillFlowContainer<DrawableMatchPairing>
                    {
                        AutoSizeAxes = Axes.X,
                        Direction = FillDirection.Vertical,
                        Children = new[]
                        {
                            new DrawableMatchPairing(pairing1),
                            new DrawableMatchPairing(pairing2),
                            new DrawableMatchPairing(new MatchPairing()),
                        }
                    },
                    level2 = new FillFlowContainer<DrawableMatchPairing>
                    {
                        AutoSizeAxes = Axes.X,
                        Direction = FillDirection.Vertical,
                        Margin = new MarginPadding(20),
                        Children = new[]
                        {
                            new DrawableMatchPairing(new MatchPairing()),
                            new DrawableMatchPairing(new MatchPairing())
                        }
                    }
                }
            };

            level1.Children[0].Pairing.Progression.Value = level2.Children[0].Pairing;
            level1.Children[1].Pairing.Progression.Value = level2.Children[0].Pairing;

            AddRepeatStep("change scores", () => pairing1.Team2Score.Value++, 4);
            AddStep("add new team", () => pairing2.Team2.Value = new TournamentTeam { FlagName = { Value = "PT" }, FullName = { Value = "Portugal" } });
            AddStep("Add progression", () => level1.Children[2].Pairing.Progression.Value = level2.Children[1].Pairing);

            AddStep("start match", () => pairing2.StartMatch());

            AddRepeatStep("change scores", () => pairing2.Team1Score.Value++, 10);

            AddStep("start submatch", () => level2.Children[0].Pairing.StartMatch());

            AddRepeatStep("change scores", () => level2.Children[0].Pairing.Team1Score.Value++, 5);

            AddRepeatStep("change scores", () => level2.Children[0].Pairing.Team2Score.Value++, 4);
        }
    }
}
