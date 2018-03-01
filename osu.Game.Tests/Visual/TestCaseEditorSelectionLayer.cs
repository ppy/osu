// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using OpenTK;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Layers.Selection;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Edit.Layers.Selection;
using osu.Game.Rulesets.Osu.Edit.Layers.Selection.Overlays;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual
{
    public class TestCaseEditorSelectionLayer : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(SelectionBox),
            typeof(SelectionLayer),
            typeof(CaptureBox),
            typeof(HitObjectComposer),
            typeof(OsuHitObjectComposer),
            typeof(HitObjectOverlayLayer),
            typeof(OsuHitObjectOverlayLayer),
            typeof(HitObjectOverlay),
            typeof(HitCircleOverlay),
            typeof(SliderOverlay),
            typeof(SliderCircleOverlay)
        };

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame)
        {
            osuGame.Beatmap.Value = new TestWorkingBeatmap(new Beatmap
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
                        Distance = 400,
                        Velocity = 1,
                        TickDistance = 100,
                        Scale = 0.5f,
                    }
                },
            });

            Child = new OsuHitObjectComposer(new OsuRuleset());
        }
    }
}
