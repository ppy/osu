// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModSpunOut : OsuModTestScene
    {
        protected override bool AllowFail => true;

        [Test]
        public void TestSpinnerAutoCompleted() => CreateModTest(new ModTestData
        {
            Mod = new OsuModSpunOut(),
            Autoplay = false,
            Beatmap = singleSpinnerBeatmap,
            PassCondition = () => Player.ChildrenOfType<DrawableSpinner>().SingleOrDefault()?.Progress >= 1
        });

        [TestCase(null)]
        [TestCase(typeof(OsuModDoubleTime))]
        [TestCase(typeof(OsuModHalfTime))]
        public void TestSpinRateUnaffectedByMods(Type additionalModType)
        {
            var mods = new List<Mod> { new OsuModSpunOut() };
            if (additionalModType != null)
                mods.Add((Mod)Activator.CreateInstance(additionalModType));

            CreateModTest(new ModTestData
            {
                Mods = mods,
                Autoplay = false,
                Beatmap = singleSpinnerBeatmap,
                PassCondition = () =>
                {
                    var counter = Player.ChildrenOfType<SpinnerSpmCalculator>().SingleOrDefault();
                    var spinner = Player.ChildrenOfType<DrawableSpinner>().FirstOrDefault();

                    if (counter == null || spinner == null)
                        return false;

                    // ignore cases where the spinner hasn't started as these lead to false-positives
                    if (Precision.AlmostEquals(counter.Result.Value, 0, 1))
                        return false;

                    double rateIndependentElapsedTime = spinner.Clock.ElapsedFrameTime / spinner.Clock.Rate;
                    float rotationSpeed = (float)(1.01 * spinner.HitObject.SpinsRequired / spinner.HitObject.Duration);

                    return Precision.AlmostEquals(counter.Result.Value, rotationSpeed * 1000 * 60, 1);
                }
            });
        }

        private Beatmap singleSpinnerBeatmap => new Beatmap
        {
            HitObjects = new List<HitObject>
            {
                new Spinner
                {
                    Position = new Vector2(256, 192),
                    StartTime = 500,
                    Duration = 2000
                }
            }
        };
    }
}
