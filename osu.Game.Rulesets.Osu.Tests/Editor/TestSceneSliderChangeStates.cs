// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Beatmaps;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public partial class TestSceneSliderChangeStates : TestSceneOsuEditor
    {
        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset, false);

        [TestCase(SplineType.Catmull)]
        [TestCase(SplineType.BSpline)]
        [TestCase(SplineType.Linear)]
        [TestCase(SplineType.PerfectCurve)]
        public void TestSliderRetainsCurveTypes(SplineType splineType)
        {
            Slider? slider = null;
            PathType pathType = new PathType(splineType);

            AddStep("add slider", () => EditorBeatmap.Add(slider = new Slider
            {
                StartTime = 500,
                Path = new SliderPath(new[]
                {
                    new PathControlPoint(Vector2.Zero, pathType),
                    new PathControlPoint(new Vector2(200, 0), pathType),
                })
            }));
            AddAssert("slider has correct spline type", () => ((Slider)EditorBeatmap.HitObjects[0]).Path.ControlPoints.All(p => p.Type == pathType));
            AddStep("remove object", () => EditorBeatmap.Remove(slider));
            AddAssert("slider removed", () => EditorBeatmap.HitObjects.Count == 0);
            addUndoSteps();
            AddAssert("slider not removed", () => EditorBeatmap.HitObjects.Count == 1);
            AddAssert("slider has correct spline type", () => ((Slider)EditorBeatmap.HitObjects[0]).Path.ControlPoints.All(p => p.Type == pathType));
        }

        private void addUndoSteps() => AddStep("undo", () => Editor.Undo());
    }
}
