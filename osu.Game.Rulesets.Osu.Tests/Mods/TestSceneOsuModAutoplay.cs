// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Rulesets.Osu.UI;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModAutoplay : OsuModTestScene
    {
        [Test]
        public void TestSpmUnaffectedByRateAdjust()
            => runSpmTest(new OsuModDaycore
            {
                SpeedChange = { Value = 0.88 }
            });

        [Test]
        public void TestSpmUnaffectedByTimeRamp()
            => runSpmTest(new ModWindUp
            {
                InitialRate = { Value = 0.7 },
                FinalRate = { Value = 1.3 }
            });

        private void runSpmTest(Mod mod)
        {
            SpinnerSpmCalculator spmCalculator = null;

            CreateModTest(new ModTestData
            {
                Autoplay = true,
                Mod = mod,
                Beatmap = new Beatmap
                {
                    HitObjects =
                    {
                        new Spinner
                        {
                            Duration = 6000,
                            Position = OsuPlayfield.BASE_SIZE / 2,
                        }
                    }
                },
                PassCondition = () => Player.ScoreProcessor.JudgedHits >= 1
            });

            AddUntilStep("fetch SPM calculator", () =>
            {
                spmCalculator = this.ChildrenOfType<SpinnerSpmCalculator>().SingleOrDefault();
                return spmCalculator != null;
            });

            AddUntilStep("SPM is correct", () => Precision.AlmostEquals(spmCalculator.Result.Value, 477, 5));
        }
    }
}
