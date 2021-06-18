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

            AddStep("retrieve first hit circle", () => hitCircle = getHitCircle(0));
            toggleAnimations(true);
            seekSmoothlyTo(() => hitCircle.StartTime + 10);

            AddAssert("hit circle piece has transforms", () =>
            {
                var drawableHitCircle = (DrawableHitCircle)getDrawableObjectFor(hitCircle);
                return getTransformsRecursively(drawableHitCircle.CirclePiece).Any(t => t.EndTime > EditorClock.CurrentTime);
            });

            AddStep("retrieve second hit circle", () => hitCircle = getHitCircle(1));
            toggleAnimations(false);
            seekSmoothlyTo(() => hitCircle.StartTime + 10);

            AddAssert("hit circle piece has no transforms", () =>
            {
                var drawableHitCircle = (DrawableHitCircle)getDrawableObjectFor(hitCircle);
                return getTransformsRecursively(drawableHitCircle.CirclePiece).All(t => t.EndTime <= EditorClock.CurrentTime);
            });
        }

        private HitCircle getHitCircle(int index)
            => EditorBeatmap.HitObjects.OfType<HitCircle>().ElementAt(index);

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
    }
}
