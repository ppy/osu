// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public partial class TestSceneOsuModAutoplay : OsuModTestScene
    {
        protected override bool AllowFail => true;

        [Test]
        public void TestCursorPositionStoredToJudgement()
        {
            CreateModTest(new ModTestData
            {
                Autoplay = true,
                PassCondition = () =>
                    Player.ScoreProcessor.JudgedHits >= 1
                    && Player.ScoreProcessor.HitEvents.Any(e => e.Position != null)
            });
        }

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

        [Test]
        public void TestPerfectScoreOnShortSliderWithRepeat()
        {
            AddStep("set score to standardised", () => LocalConfig.SetValue(OsuSetting.ScoreDisplayMode, ScoringMode.Standardised));

            CreateModTest(new ModTestData
            {
                Autoplay = true,
                Beatmap = new Beatmap
                {
                    HitObjects = new List<HitObject>
                    {
                        new Slider
                        {
                            StartTime = 500,
                            Position = new Vector2(256, 192),
                            Path = new SliderPath(new[]
                            {
                                new PathControlPoint(),
                                new PathControlPoint(new Vector2(0, 6.25f))
                            }),
                            RepeatCount = 1,
                            SliderVelocityMultiplier = 10
                        }
                    }
                },
                PassCondition = () => Player.ScoreProcessor.TotalScore.Value == 1_000_000
            });
        }

        private void runSpmTest(Mod mod)
        {
            SpinnerSpmCalculator? spmCalculator = null;

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

            AddUntilStep("SPM is correct", () => Precision.AlmostEquals(spmCalculator.AsNonNull().Result.Value, 477, 5));
        }
    }
}
