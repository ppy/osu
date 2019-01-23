// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestCaseSliderSelectionBlueprint : SelectionBlueprintTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(SliderSelectionBlueprint),
            typeof(SliderCircleSelectionBlueprint),
            typeof(SliderBodyPiece),
            typeof(SliderCircle),
            typeof(PathControlPointVisualiser),
            typeof(PathControlPointPiece)
        };

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            var configCache = dependencies.Get<RulesetConfigCache>();
            dependencies.CacheAs((OsuConfigManager)configCache.GetConfigFor(new OsuRuleset()));

            return dependencies;
        }

        private readonly DrawableSlider drawableObject;

        public TestCaseSliderSelectionBlueprint()
        {
            var slider = new Slider
            {
                Position = new Vector2(256, 192),
                Path = new SliderPath(PathType.Bezier, new[]
                {
                    Vector2.Zero,
                    new Vector2(150, 150),
                    new Vector2(300, 0)
                })
            };

            slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = 2 });

            Add(drawableObject = new DrawableSlider(slider));
        }

        protected override SelectionBlueprint CreateBlueprint() => new SliderSelectionBlueprint(drawableObject);
    }
}
