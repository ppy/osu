// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public class TestSceneSliderStreamConversion : TestSceneOsuEditor
    {
        private BindableBeatDivisor beatDivisor => (BindableBeatDivisor)Editor.Dependencies.Get(typeof(BindableBeatDivisor));

        [Test]
        public void TestSimpleConversion()
        {
            Slider slider = null;

            AddStep("select first slider", () =>
            {
                slider = (Slider)EditorBeatmap.HitObjects.First(h => h is Slider);
                EditorClock.Seek(slider.StartTime);
                EditorBeatmap.SelectedHitObjects.Add(slider);
            });

            convertToStream();

            AddAssert("stream created", () => streamCreatedFor(slider,
                (time: 0, pathPosition: 0),
                (time: 0.25, pathPosition: 0.25),
                (time: 0.5, pathPosition: 0.5),
                (time: 0.75, pathPosition: 0.75),
                (time: 1, pathPosition: 1)));

            AddStep("undo", () => Editor.Undo());
            AddAssert("slider restored", () => sliderRestored(slider));

            AddStep("select first slider", () =>
            {
                slider = (Slider)EditorBeatmap.HitObjects.First(h => h is Slider);
                EditorClock.Seek(slider.StartTime);
                EditorBeatmap.SelectedHitObjects.Add(slider);
            });
            AddStep("change beat divisor", () => beatDivisor.Value = 8);

            convertToStream();
            AddAssert("stream created", () => streamCreatedFor(slider,
                (time: 0, pathPosition: 0),
                (time: 0.125, pathPosition: 0.125),
                (time: 0.25, pathPosition: 0.25),
                (time: 0.375, pathPosition: 0.375),
                (time: 0.5, pathPosition: 0.5),
                (time: 0.625, pathPosition: 0.625),
                (time: 0.75, pathPosition: 0.75),
                (time: 0.875, pathPosition: 0.875),
                (time: 1, pathPosition: 1)));
        }

        [Test]
        public void TestConversionWithNonMatchingDivisor()
        {
            Slider slider = null;

            AddStep("select second slider", () =>
            {
                slider = (Slider)EditorBeatmap.HitObjects.Where(h => h is Slider).ElementAt(1);
                EditorClock.Seek(slider.StartTime);
                EditorBeatmap.SelectedHitObjects.Add(slider);
            });
            AddStep("change beat divisor", () => beatDivisor.Value = 3);

            convertToStream();

            AddAssert("stream created", () => streamCreatedFor(slider,
                (time: 0, pathPosition: 0),
                (time: 2 / 3d, pathPosition: 2 / 3d)));
        }

        [Test]
        public void TestConversionWithRepeats()
        {
            Slider slider = null;

            AddStep("select first slider with repeats", () =>
            {
                slider = (Slider)EditorBeatmap.HitObjects.First(h => h is Slider s && s.RepeatCount > 0);
                EditorClock.Seek(slider.StartTime);
                EditorBeatmap.SelectedHitObjects.Add(slider);
            });
            AddStep("change beat divisor", () => beatDivisor.Value = 2);

            convertToStream();

            AddAssert("stream created", () => streamCreatedFor(slider,
                (time: 0, pathPosition: 0),
                (time: 0.25, pathPosition: 0.5),
                (time: 0.5, pathPosition: 1),
                (time: 0.75, pathPosition: 0.5),
                (time: 1, pathPosition: 0)));
        }

        [Test]
        public void TestConversionPreservesSliderProperties()
        {
            Slider slider = null;

            AddStep("select second new-combo-starting slider", () =>
            {
                slider = (Slider)EditorBeatmap.HitObjects.Where(h => h is Slider s && s.NewCombo).ElementAt(1);
                EditorClock.Seek(slider.StartTime);
                EditorBeatmap.SelectedHitObjects.Add(slider);
            });

            convertToStream();

            AddAssert("stream created", () => streamCreatedFor(slider,
                (time: 0, pathPosition: 0),
                (time: 0.25, pathPosition: 0.25),
                (time: 0.5, pathPosition: 0.5),
                (time: 0.75, pathPosition: 0.75),
                (time: 1, pathPosition: 1)));

            AddStep("undo", () => Editor.Undo());
            AddAssert("slider restored", () => sliderRestored(slider));
        }

        private void convertToStream()
        {
            AddStep("convert to stream", () =>
            {
                InputManager.PressKey(Key.LControl);
                InputManager.PressKey(Key.LShift);
                InputManager.Key(Key.F);
                InputManager.ReleaseKey(Key.LShift);
                InputManager.ReleaseKey(Key.LControl);
            });
        }

        private bool streamCreatedFor(Slider slider, params (double time, double pathPosition)[] expectedCircles)
        {
            if (EditorBeatmap.HitObjects.Contains(slider))
                return false;

            foreach ((double expectedTime, double expectedPathPosition) in expectedCircles)
            {
                double time = slider.StartTime + slider.Duration * expectedTime;
                Vector2 position = slider.Position + slider.Path.PositionAt(expectedPathPosition);

                if (!EditorBeatmap.HitObjects.OfType<HitCircle>().Any(h => matches(h, time, position, slider.NewCombo && expectedTime == 0)))
                    return false;
            }

            return true;

            bool matches(HitCircle circle, double time, Vector2 position, bool startsNewCombo) =>
                Precision.AlmostEquals(circle.StartTime, time, 1)
                && Precision.AlmostEquals(circle.Position, position, 0.01f)
                && circle.NewCombo == startsNewCombo
                && circle.Samples.SequenceEqual(slider.HeadCircle.Samples)
                && circle.SampleControlPoint.IsRedundant(slider.SampleControlPoint);
        }

        private bool sliderRestored(Slider slider)
        {
            var objects = EditorBeatmap.HitObjects.Where(h => h.StartTime >= slider.StartTime && h.GetEndTime() <= slider.EndTime).ToList();

            if (objects.Count > 1)
                return false;

            var hitObject = objects.Single();
            if (!(hitObject is Slider restoredSlider))
                return false;

            return Precision.AlmostEquals(slider.StartTime, restoredSlider.StartTime)
                   && Precision.AlmostEquals(slider.GetEndTime(), restoredSlider.GetEndTime())
                   && Precision.AlmostEquals(slider.Position, restoredSlider.Position, 0.01f)
                   && Precision.AlmostEquals(slider.EndPosition, restoredSlider.EndPosition, 0.01f);
        }
    }
}
