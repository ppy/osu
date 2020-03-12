// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Screens.Gameplay.Components;
using osuTK;

namespace osu.Game.Tournament.Tests.Components
{
    public class TestSceneMatchHeader : TournamentTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DrawableTournamentHeaderText),
            typeof(DrawableTournamentHeaderLogo),
        };

        public TestSceneMatchHeader()
        {
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(50),
                Children = new Drawable[]
                {
                    new TournamentSpriteText { Text = "with logo", Font = OsuFont.Torus.With(size: 30) },
                    new MatchHeader(),
                    new TournamentSpriteText { Text = "without logo", Font = OsuFont.Torus.With(size: 30) },
                    new MatchHeader { ShowLogo = false },
                    new TournamentSpriteText { Text = "without scores", Font = OsuFont.Torus.With(size: 30) },
                    new MatchHeader { ShowScores = false },
                }
            };
        }
    }
}
