// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    [TestFixture]
    public class TestSceneOsuEditorHitAnimations : TestSceneOsuEditor
    {
        [Resolved]
        private OsuConfigManager config { get; set; }

        [Test]
        public void TestHitCircleAnimationDisable()
        {
            HitCircle hitCircle = null;
            DrawableHitCircle drawableHitCircle = null;

            AddStep("retrieve first hit circle", () => hitCircle = getHitCircle(0));
            toggleAnimations(true);
            seekSmoothlyTo(() => hitCircle.StartTime + 10);

            AddStep("retrieve drawable", () => drawableHitCircle = (DrawableHitCircle)getDrawableObjectFor(hitCircle));
            assertFutureTransforms(() => drawableHitCircle.CirclePiece, true);

            AddStep("retrieve second hit circle", () => hitCircle = getHitCircle(1));
            toggleAnimations(false);
            seekSmoothlyTo(() => hitCircle.StartTime + 10);

            AddStep("retrieve drawable", () => drawableHitCircle = (DrawableHitCircle)getDrawableObjectFor(hitCircle));
            assertFutureTransforms(() => drawableHitCircle.CirclePiece, false);
            AddAssert("hit circle has longer fade-out applied", () =>
            {
                var alphaTransform = drawableHitCircle.Transforms.Last(t => t.TargetMember == nameof(Alpha));
                return alphaTransform.EndTime - alphaTransform.StartTime == DrawableOsuEditorRuleset.EDITOR_HIT_OBJECT_FADE_OUT_EXTENSION;
            });
        }

        [Test]
        public void TestSliderAnimationDisable()
        {
            Slider slider = null;
            DrawableSlider drawableSlider = null;
            DrawableSliderRepeat sliderRepeat = null;

            AddStep("retrieve first slider with repeats", () => slider = getSliderWithRepeats(0));
            toggleAnimations(true);
            seekSmoothlyTo(() => slider.StartTime + slider.SpanDuration + 10);

            retrieveDrawables();
            assertFutureTransforms(() => sliderRepeat, true);

            AddStep("retrieve second slider with repeats", () => slider = getSliderWithRepeats(1));
            toggleAnimations(false);
            seekSmoothlyTo(() => slider.StartTime + slider.SpanDuration + 10);

            retrieveDrawables();
            assertFutureTransforms(() => sliderRepeat.Arrow, false);
            seekSmoothlyTo(() => slider.GetEndTime());
            AddAssert("slider has longer fade-out applied", () =>
            {
                var alphaTransform = drawableSlider.Transforms.Last(t => t.TargetMember == nameof(Alpha));
                return alphaTransform.EndTime - alphaTransform.StartTime == DrawableOsuEditorRuleset.EDITOR_HIT_OBJECT_FADE_OUT_EXTENSION;
            });

            void retrieveDrawables() =>
                AddStep("retrieve drawables", () =>
                {
                    drawableSlider = (DrawableSlider)getDrawableObjectFor(slider);
                    sliderRepeat = (DrawableSliderRepeat)getDrawableObjectFor(slider.NestedHitObjects.OfType<SliderRepeat>().First());
                });
        }

        private HitCircle getHitCircle(int index)
            => EditorBeatmap.HitObjects.OfType<HitCircle>().ElementAt(index);

        private Slider getSliderWithRepeats(int index)
            => EditorBeatmap.HitObjects.OfType<Slider>().Where(s => s.RepeatCount >= 1).ElementAt(index);

        private DrawableHitObject getDrawableObjectFor(HitObject hitObject)
            => this.ChildrenOfType<DrawableHitObject>().Single(ho => ho.HitObject == hitObject);

        private IEnumerable<Transform> getTransformsRecursively(Drawable drawable)
            => drawable.ChildrenOfType<Drawable>().SelectMany(d => d.Transforms);

        private void toggleAnimations(bool enabled)
            => AddStep($"toggle animations {(enabled ? "on" : "off")}", () => config.SetValue(OsuSetting.EditorHitAnimations, enabled));

        private void seekSmoothlyTo(Func<double> targetTime)
        {
            AddStep("seek smoothly", () => EditorClock.SeekSmoothlyTo(targetTime.Invoke()));
            AddUntilStep("wait for seek", () => Precision.AlmostEquals(targetTime.Invoke(), EditorClock.CurrentTime));
        }

        private void assertFutureTransforms(Func<Drawable> getDrawable, bool hasFutureTransforms)
            => AddAssert($"object {(hasFutureTransforms ? "has" : "has no")} future transforms",
                () => getTransformsRecursively(getDrawable()).Any(t => t.EndTime >= EditorClock.CurrentTime) == hasFutureTransforms);
    }
}
