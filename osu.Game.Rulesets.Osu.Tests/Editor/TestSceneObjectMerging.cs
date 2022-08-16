// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public class TestSceneObjectMerging : TestSceneOsuEditor
    {
        [Test]
        public void TestSimpleMerge()
        {
            HitCircle? circle1 = null;
            HitCircle? circle2 = null;

            AddStep("select first two circles", () =>
            {
                circle1 = (HitCircle)EditorBeatmap.HitObjects.First(h => h is HitCircle);
                circle2 = (HitCircle)EditorBeatmap.HitObjects.First(h => h is HitCircle && h != circle1);
                EditorClock.Seek(circle1.StartTime);
                EditorBeatmap.SelectedHitObjects.Add(circle1);
                EditorBeatmap.SelectedHitObjects.Add(circle2);
            });

            mergeSelection();

            AddAssert("slider created", () => circle1 is not null && circle2 is not null && sliderCreatedFor(
                (pos: circle1.Position, pathType: PathType.Linear),
                (pos: circle2.Position, pathType: null)));

            AddStep("undo", () => Editor.Undo());
            AddAssert("merged objects restored", () => circle1 is not null && circle2 is not null && objectsRestored(circle1, circle2));
        }

        [Test]
        public void TestMergeCircleSlider()
        {
            HitCircle? circle1 = null;
            Slider? slider = null;
            HitCircle? circle2 = null;

            AddStep("select a circle, slider, circle", () =>
            {
                circle1 = (HitCircle)EditorBeatmap.HitObjects.First(h => h is HitCircle);
                slider = (Slider)EditorBeatmap.HitObjects.First(h => h is Slider && h.StartTime > circle1.StartTime);
                circle2 = (HitCircle)EditorBeatmap.HitObjects.First(h => h is HitCircle && h.StartTime > slider.StartTime);
                EditorClock.Seek(circle1.StartTime);
                EditorBeatmap.SelectedHitObjects.Add(circle1);
                EditorBeatmap.SelectedHitObjects.Add(slider);
                EditorBeatmap.SelectedHitObjects.Add(circle2);
            });

            mergeSelection();

            AddAssert("slider created", () =>
            {
                if (circle1 is null || circle2 is null || slider is null)
                    return false;

                var controlPoints = slider.Path.ControlPoints;
                (Vector2, PathType?)[] args = new (Vector2, PathType?)[controlPoints.Count + 2];
                args[0] = (circle1.Position, PathType.Linear);

                for (int i = 0; i < controlPoints.Count; i++)
                {
                    args[i + 1] = (controlPoints[i].Position + slider.Position, i == controlPoints.Count - 1 ? PathType.Linear : controlPoints[i].Type);
                }

                args[^1] = (circle2.Position, null);
                return sliderCreatedFor(args);
            });

            AddStep("undo", () => Editor.Undo());
            AddAssert("merged objects restored", () => circle1 is not null && circle2 is not null && slider is not null && objectsRestored(circle1, slider, circle2));
        }

        [Test]
        public void TestMergeSliderSlider()
        {
            Slider? slider1 = null;
            SliderPath? slider1Path = null;
            Slider? slider2 = null;

            AddStep("select two sliders", () =>
            {
                slider1 = (Slider)EditorBeatmap.HitObjects.First(h => h is Slider);
                slider1Path = new SliderPath(slider1.Path.ControlPoints.Select(p => new PathControlPoint(p.Position, p.Type)).ToArray(), slider1.Path.ExpectedDistance.Value);
                slider2 = (Slider)EditorBeatmap.HitObjects.First(h => h is Slider && h.StartTime > slider1.StartTime);
                EditorClock.Seek(slider1.StartTime);
                EditorBeatmap.SelectedHitObjects.Add(slider1);
                EditorBeatmap.SelectedHitObjects.Add(slider2);
            });

            mergeSelection();

            AddAssert("slider created", () =>
            {
                if (slider1 is null || slider2 is null || slider1Path is null)
                    return false;

                var controlPoints1 = slider1Path.ControlPoints;
                var controlPoints2 = slider2.Path.ControlPoints;
                (Vector2, PathType?)[] args = new (Vector2, PathType?)[controlPoints1.Count + controlPoints2.Count - 1];

                for (int i = 0; i < controlPoints1.Count - 1; i++)
                {
                    args[i] = (controlPoints1[i].Position + slider1.Position, controlPoints1[i].Type);
                }

                for (int i = 0; i < controlPoints2.Count; i++)
                {
                    args[i + controlPoints1.Count - 1] = (controlPoints2[i].Position + controlPoints1[^1].Position + slider1.Position, controlPoints2[i].Type);
                }

                return sliderCreatedFor(args);
            });

            AddAssert("merged slider matches first slider", () =>
            {
                var mergedSlider = (Slider)EditorBeatmap.SelectedHitObjects.First();
                return slider1 is not null && mergedSlider.HeadCircle.Samples.SequenceEqual(slider1.HeadCircle.Samples)
                                           && mergedSlider.TailCircle.Samples.SequenceEqual(slider1.TailCircle.Samples)
                                           && mergedSlider.Samples.SequenceEqual(slider1.Samples)
                                           && mergedSlider.SampleControlPoint.IsRedundant(slider1.SampleControlPoint);
            });

            AddAssert("slider end is at same completion for last slider", () =>
            {
                if (slider1Path is null || slider2 is null)
                    return false;

                var mergedSlider = (Slider)EditorBeatmap.SelectedHitObjects.First();
                return Precision.AlmostEquals(mergedSlider.Path.Distance, slider1Path.CalculatedDistance + slider2.Path.Distance);
            });
        }

        [Test]
        public void TestNonMerge()
        {
            HitCircle? circle1 = null;
            HitCircle? circle2 = null;
            Spinner? spinner = null;

            AddStep("select first two circles and spinner", () =>
            {
                circle1 = (HitCircle)EditorBeatmap.HitObjects.First(h => h is HitCircle);
                circle2 = (HitCircle)EditorBeatmap.HitObjects.First(h => h is HitCircle && h != circle1);
                spinner = (Spinner)EditorBeatmap.HitObjects.First(h => h is Spinner);
                EditorClock.Seek(spinner.StartTime);
                EditorBeatmap.SelectedHitObjects.Add(circle1);
                EditorBeatmap.SelectedHitObjects.Add(circle2);
                EditorBeatmap.SelectedHitObjects.Add(spinner);
            });

            mergeSelection();

            AddAssert("slider created", () => circle1 is not null && circle2 is not null && sliderCreatedFor(
                (pos: circle1.Position, pathType: PathType.Linear),
                (pos: circle2.Position, pathType: null)));

            AddAssert("spinner not merged", () => EditorBeatmap.HitObjects.Contains(spinner));
        }

        private void mergeSelection()
        {
            AddStep("merge selection", () =>
            {
                InputManager.PressKey(Key.LControl);
                InputManager.PressKey(Key.LShift);
                InputManager.Key(Key.M);
                InputManager.ReleaseKey(Key.LShift);
                InputManager.ReleaseKey(Key.LControl);
            });
        }

        private bool sliderCreatedFor(params (Vector2 pos, PathType? pathType)[] expectedControlPoints)
        {
            if (EditorBeatmap.SelectedHitObjects.Count != 1)
                return false;

            var mergedSlider = (Slider)EditorBeatmap.SelectedHitObjects.First();
            int i = 0;

            foreach ((Vector2 pos, PathType? pathType) in expectedControlPoints)
            {
                var controlPoint = mergedSlider.Path.ControlPoints[i++];

                if (!Precision.AlmostEquals(controlPoint.Position + mergedSlider.Position, pos) || controlPoint.Type != pathType)
                    return false;
            }

            return true;
        }

        private bool objectsRestored(params HitObject[] objects)
        {
            foreach (var hitObject in objects)
            {
                if (EditorBeatmap.HitObjects.Contains(hitObject))
                    return false;
            }

            return true;
        }
    }
}
