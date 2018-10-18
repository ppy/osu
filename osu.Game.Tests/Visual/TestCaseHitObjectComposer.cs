// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Timing;
using OpenTK;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Edit.Masks;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Screens.Compose;
using osu.Game.Screens.Edit.Screens.Compose.Layers;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    [Cached(Type = typeof(IPlacementHandler))]
    public class TestCaseHitObjectComposer : EditorClockTestCase, IPlacementHandler
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(MaskSelection),
            typeof(DragLayer),
            typeof(HitObjectComposer),
            typeof(OsuHitObjectComposer),
            typeof(HitObjectMaskLayer),
            typeof(NotNullAttribute),
            typeof(HitCircleMask),
            typeof(HitCircleSelectionMask),
            typeof(HitCirclePlacementMask),
        };

        public TestCaseHitObjectComposer()
        {
            BeatDivisor.Value = 4;
        }

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
                        ControlPoints = new List<Vector2>
                        {
                            Vector2.Zero,
                            new Vector2(216, 0),
                        },
                        Distance = 216,
                        Velocity = 1,
                        TickDistance = 100,
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
