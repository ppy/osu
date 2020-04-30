// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Tests.Components
{
    public class TestSceneRoundDisplay : TournamentTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DrawableTournamentHeaderText),
            typeof(DrawableTournamentHeaderLogo),
        };

        public TestSceneRoundDisplay()
        {
            Children = new Drawable[]
            {
                new RoundDisplay(new TournamentMatch
                {
                    Round =
                    {
                        Value = new TournamentRound
                        {
                            Name = { Value = "Test Round" }
                        }
                    }
                })
                {
                    Margin = new MarginPadding(20)
                }
            };
        }
    }
}
