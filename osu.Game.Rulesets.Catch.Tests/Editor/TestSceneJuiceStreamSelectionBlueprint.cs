// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.Edit.Blueprints;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Catch.Tests.Editor
{
    public class TestSceneJuiceStreamSelectionBlueprint : CatchSelectionBlueprintTestScene
    {
        public TestSceneJuiceStreamSelectionBlueprint()
        {
            var hitObject = new JuiceStream
            {
                OriginalX = 100,
                StartTime = 100,
                Path = new SliderPath(PathType.PerfectCurve, new[]
                {
                    Vector2.Zero,
                    new Vector2(200, 100),
                    new Vector2(0, 200),
                }),
            };
            var controlPoint = new ControlPointInfo();
            controlPoint.Add(0, new TimingControlPoint
            {
                BeatLength = 100
            });
            hitObject.ApplyDefaults(controlPoint, new BeatmapDifficulty { CircleSize = 0 });
            AddBlueprint(new JuiceStreamSelectionBlueprint(hitObject));
        }
    }
}
