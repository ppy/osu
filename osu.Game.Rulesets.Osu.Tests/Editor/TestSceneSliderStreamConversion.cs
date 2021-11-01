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

            AddAssert("stream created", () => streamCreatedFor(slider, 1 / 4d));

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
            AddAssert("stream created", () => streamCreatedFor(slider, 1 / 8d));
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

            AddAssert("stream created", () => streamCreatedFor(slider, 2 / 3d));
        }

        [Test]
        public void TestConversionPreservesNewCombo()
        {
            Slider slider = null;

            AddStep("select second new-combo-starting slider", () =>
            {
                slider = (Slider)EditorBeatmap.HitObjects.Where(h => h is Slider s && s.NewCombo).ElementAt(1);
                EditorClock.Seek(slider.StartTime);
                EditorBeatmap.SelectedHitObjects.Add(slider);
            });

            convertToStream();

            AddAssert("stream created", () => streamCreatedFor(slider, 1 / 4d));

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

        private bool streamCreatedFor(Slider slider, double spacing)
        {
            if (EditorBeatmap.HitObjects.Contains(slider))
                return false;

            for (int i = 0; i * spacing <= 1; ++i)
            {
                double progress = i * spacing;
                double time = slider.StartTime + progress * slider.Duration;
                Vector2 position = slider.Position + slider.Path.PositionAt(progress);

                if (!EditorBeatmap.HitObjects.OfType<HitCircle>().Any(h => matches(h, time, position, slider.NewCombo && progress == 0)))
                    return false;
            }

            return true;

            bool matches(HitCircle circle, double time, Vector2 position, bool startsNewCombo) =>
                Precision.AlmostEquals(circle.StartTime, time, 1)
                && Precision.AlmostEquals(circle.Position, position, 0.01f)
                && circle.NewCombo == startsNewCombo;
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
