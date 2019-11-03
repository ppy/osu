// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Spinners;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Spinners.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneSpinnerSelectionBlueprint : SelectionBlueprintTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(SpinnerSelectionBlueprint),
            typeof(SpinnerPiece)
        };

        public TestSceneSpinnerSelectionBlueprint()
        {
            var spinner = new Spinner
            {
                Position = new Vector2(256, 256),
                StartTime = -1000,
                EndTime = 2000
            };

            spinner.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = 2 });

            DrawableSpinner drawableSpinner;

            Add(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.5f),
                Child = drawableSpinner = new DrawableSpinner(spinner)
            });

            AddBlueprint(new SpinnerSelectionBlueprint(drawableSpinner) { Size = new Vector2(0.5f) });
        }
    }
}
