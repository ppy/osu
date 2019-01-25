// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Timing;
using osuTK;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    [Cached(Type = typeof(IPlacementHandler))]
    public class TestCaseHitObjectComposer : OsuTestCase, IPlacementHandler
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(SelectionHandler),
            typeof(DragBox),
            typeof(HitObjectComposer),
            typeof(OsuHitObjectComposer),
            typeof(BlueprintContainer),
            typeof(NotNullAttribute),
            typeof(HitCirclePiece),
            typeof(HitCircleSelectionBlueprint),
            typeof(HitCirclePlacementBlueprint),
        };

        private HitObjectComposer composer;

        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.Value = new TestWorkingBeatmap(new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { Position = new Vector2(256, 192), Scale = 0.5f },
                    new HitCircle { Position = new Vector2(344, 148), Scale = 0.5f },
                    new Slider
                    {
                        Position = new Vector2(128, 256),
                        Path = new SliderPath(PathType.Linear, new[]
                        {
                            Vector2.Zero,
                            new Vector2(216, 0),
                        }),
                        Scale = 0.5f,
                    }
                },
            });

            var clock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };
            Dependencies.CacheAs<IAdjustableClock>(clock);
            Dependencies.CacheAs<IFrameBasedClock>(clock);

            Child = composer = new OsuHitObjectComposer(new OsuRuleset());
        }

        public void BeginPlacement(HitObject hitObject)
        {
        }

        public void EndPlacement(HitObject hitObject) => composer.Add(hitObject);

        public void Delete(HitObject hitObject) => composer.Remove(hitObject);
    }
}
