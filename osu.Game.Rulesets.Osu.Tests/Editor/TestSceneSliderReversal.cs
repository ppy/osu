// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
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
                PathType.PerfectCurve,
                new Vector2(200, -50),
                new Vector2(250, 0)
            ),
            createPathSegment(
                PathType.Linear,
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

        [TestCase(0, 250)]
        [TestCase(0, 200)]
        [TestCase(1, 80)]
        [TestCase(1, 120)]
        public void TestSliderReversal(int pathIndex, double length)
        {
            var controlPoints = paths[pathIndex];

            Vector2 oldStartPos = default;
            Vector2 oldEndPos = default;
            double oldDistance = default;

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

            AddAssert("Slider was reversed correctly", () =>
            {
                var slider = (Slider)EditorBeatmap.SelectedHitObjects[0];
                return Vector2.Distance(slider.Position, oldEndPos) < 1
                       && Vector2.Distance(slider.EndPosition, oldStartPos) < 1
                       && Math.Abs(slider.Path.Distance - oldDistance) < 1e-10;
            });
        }
    }
}
