// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public partial class TestSceneSliderReversal : TestSceneOsuEditor
    {
        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(Ruleset.Value, false);

        private readonly PathControlPoint[][] paths =
        {
            createPathSegment(
                PathType.PERFECTCURVE,
                new Vector2(200, -50),
                new Vector2(250, 0)
            ),
            createPathSegment(
                PathType.LINEAR,
                new Vector2(100, 0),
                new Vector2(100, 100)
            )
        };

        private static PathControlPoint[] createPathSegment(PathType type, params Vector2[] positions)
        {
            return positions.Select(p => new PathControlPoint
            {
                Position = p
            }).Prepend(new PathControlPoint
            {
                Type = type
            }).ToArray();
        }

        private Slider selectedSlider => (Slider)EditorBeatmap.SelectedHitObjects[0];

        [TestCase(0, 250)]
        [TestCase(0, 200)]
        [TestCase(1, 120)]
        [TestCase(1, 80)]
        public void TestSliderReversal(int pathIndex, double length)
        {
            var controlPoints = paths[pathIndex];

            Vector2 oldStartPos = default;
            Vector2 oldEndPos = default;
            double oldDistance = default;
            var oldControlPointTypes = controlPoints.Select(p => p.Type);

            AddStep("Add slider", () =>
            {
                var slider = new Slider
                {
                    Position = new Vector2(OsuPlayfield.BASE_SIZE.X / 2, OsuPlayfield.BASE_SIZE.Y / 2),
                    Path = new SliderPath(controlPoints)
                    {
                        ExpectedDistance = { Value = length }
                    }
                };

                EditorBeatmap.Add(slider);

                oldStartPos = slider.Position;
                oldEndPos = slider.EndPosition;
                oldDistance = slider.Path.Distance;
            });

            AddStep("Select slider", () =>
            {
                var slider = (Slider)EditorBeatmap.HitObjects[0];
                EditorBeatmap.SelectedHitObjects.Add(slider);
            });

            AddStep("Reverse slider", () =>
            {
                InputManager.PressKey(Key.LControl);
                InputManager.Key(Key.G);
                InputManager.ReleaseKey(Key.LControl);
            });

            AddAssert("Slider has correct length", () =>
                Precision.AlmostEquals(selectedSlider.Path.Distance, oldDistance));

            AddAssert("Slider has correct start position", () =>
                Vector2.Distance(selectedSlider.Position, oldEndPos) < 1);

            AddAssert("Slider has correct end position", () =>
                Vector2.Distance(selectedSlider.EndPosition, oldStartPos) < 1);

            AddAssert("Control points have correct types", () =>
            {
                var newControlPointTypes = selectedSlider.Path.ControlPoints.Select(p => p.Type).ToArray();

                return oldControlPointTypes.Take(newControlPointTypes.Length).SequenceEqual(newControlPointTypes);
            });
        }
    }
}
