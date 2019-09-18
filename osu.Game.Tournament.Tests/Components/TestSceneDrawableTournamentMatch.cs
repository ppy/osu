// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Ladder.Components;

namespace osu.Game.Tournament.Tests.Components
{
    public class TestSceneDrawableTournamentMatch : TournamentTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(TournamentMatch),
            typeof(DrawableTournamentTeam),
        };

        public TestSceneDrawableTournamentMatch()
        {
            Container<DrawableTournamentMatch> level1;
            Container<DrawableTournamentMatch> level2;

            var match1 = new TournamentMatch(
                new TournamentTeam { FlagName = { Value = "AU" }, FullName = { Value = "Australia" }, },
                new TournamentTeam { FlagName = { Value = "JP" }, FullName = { Value = "Japan" }, Acronym = { Value = "JPN" } })
            {
                Team1Score = { Value = 4 },
                Team2Score = { Value = 1 },
            };

            var match2 = new TournamentMatch(
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
                    level1 = new FillFlowContainer<DrawableTournamentMatch>
                    {
                        AutoSizeAxes = Axes.X,
                        Direction = FillDirection.Vertical,
                        Children = new[]
                        {
                            new DrawableTournamentMatch(match1),
                            new DrawableTournamentMatch(match2),
                            new DrawableTournamentMatch(new TournamentMatch()),
                        }
                    },
                    level2 = new FillFlowContainer<DrawableTournamentMatch>
                    {
                        AutoSizeAxes = Axes.X,
                        Direction = FillDirection.Vertical,
                        Margin = new MarginPadding(20),
                        Children = new[]
                        {
                            new DrawableTournamentMatch(new TournamentMatch()),
                            new DrawableTournamentMatch(new TournamentMatch())
                        }
                    }
                }
            };

            level1.Children[0].Match.Progression.Value = level2.Children[0].Match;
            level1.Children[1].Match.Progression.Value = level2.Children[0].Match;

            AddRepeatStep("change scores", () => match1.Team2Score.Value++, 4);
            AddStep("add new team", () => match2.Team2.Value = new TournamentTeam { FlagName = { Value = "PT" }, FullName = { Value = "Portugal" } });
            AddStep("Add progression", () => level1.Children[2].Match.Progression.Value = level2.Children[1].Match);

            AddStep("start match", () => match2.StartMatch());

            AddRepeatStep("change scores", () => match2.Team1Score.Value++, 10);

            AddStep("start submatch", () => level2.Children[0].Match.StartMatch());

            AddRepeatStep("change scores", () => level2.Children[0].Match.Team1Score.Value++, 5);

            AddRepeatStep("change scores", () => level2.Children[0].Match.Team2Score.Value++, 4);
        }
    }
}
