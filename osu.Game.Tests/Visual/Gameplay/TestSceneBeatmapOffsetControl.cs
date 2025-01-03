// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play.PlayerSettings;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual.Ranking;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneBeatmapOffsetControl : OsuTestScene
    {
        private BeatmapOffsetControl offsetControl = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            recreateControl();
        }

        [Test]
        public void TestTooShortToDisplay()
        {
            AddStep("Set short reference score", () =>
            {
                offsetControl.ReferenceScore.Value = new ScoreInfo
                {
                    HitEvents = TestSceneHitEventTimingDistributionGraph.CreateDistributedHitEvents(0, 2),
                    BeatmapInfo = Beatmap.Value.BeatmapInfo,
                };
            });

            AddAssert("No calibration button", () => !offsetControl.ChildrenOfType<SettingsButton>().Any());
        }

        [Test]
        public void TestNotEnoughTimedHitEvents()
        {
            AddStep("Set short reference score", () =>
            {
                List<HitEvent> hitEvents =
                [
                    // 10 events total. one of them (head circle) being timed / having hitwindows, rest having no hitwindows
                    new HitEvent(30, 1, HitResult.LargeTickHit, new SliderHeadCircle { ClassicSliderBehaviour = true }, null, null),
                    new HitEvent(0, 1, HitResult.LargeTickHit, new SliderTick(), null, null),
                    new HitEvent(0, 1, HitResult.LargeTickHit, new SliderTick(), null, null),
                    new HitEvent(0, 1, HitResult.LargeTickHit, new SliderTick(), null, null),
                    new HitEvent(0, 1, HitResult.LargeTickHit, new SliderTick(), null, null),
                    new HitEvent(0, 1, HitResult.LargeTickHit, new SliderTick(), null, null),
                    new HitEvent(0, 1, HitResult.LargeTickHit, new SliderTick(), null, null),
                    new HitEvent(0, 1, HitResult.LargeTickHit, new SliderTick(), null, null),
                    new HitEvent(0, 1, HitResult.LargeTickHit, new SliderTick(), null, null),
                    new HitEvent(0, 1, HitResult.LargeTickHit, new SliderTick(), null, null),
                ];

                foreach (var ev in hitEvents)
                    ev.HitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                offsetControl.ReferenceScore.Value = new ScoreInfo
                {
                    HitEvents = hitEvents,
                    BeatmapInfo = Beatmap.Value.BeatmapInfo,
                };
            });

            AddAssert("No calibration button", () => !offsetControl.ChildrenOfType<SettingsButton>().Any());
        }

        [Test]
        public void TestScoreFromDifferentBeatmap()
        {
            AddStep("Set short reference score", () =>
            {
                offsetControl.ReferenceScore.Value = new ScoreInfo
                {
                    HitEvents = TestSceneHitEventTimingDistributionGraph.CreateDistributedHitEvents(10),
                    BeatmapInfo = TestResources.CreateTestBeatmapSetInfo().Beatmaps.First(),
                };
            });

            AddAssert("No calibration button", () => !offsetControl.ChildrenOfType<SettingsButton>().Any());
        }

        [Test]
        public void TestModRemovingTimedInputs()
        {
            AddStep("Set score with mod removing timed inputs", () =>
            {
                offsetControl.ReferenceScore.Value = new ScoreInfo
                {
                    HitEvents = TestSceneHitEventTimingDistributionGraph.CreateDistributedHitEvents(10),
                    Mods = new Mod[] { new OsuModRelax() },
                    BeatmapInfo = Beatmap.Value.BeatmapInfo,
                };
            });

            AddAssert("No calibration button", () => !offsetControl.ChildrenOfType<SettingsButton>().Any());
        }

        [Test]
        public void TestCalibrationFromZero()
        {
            ScoreInfo referenceScore = null!;
            const double average_error = -4.5;

            AddAssert("Offset is neutral", () => offsetControl.Current.Value == 0);
            AddAssert("No calibration button", () => !offsetControl.ChildrenOfType<SettingsButton>().Any());
            AddStep("Set reference score", () =>
            {
                offsetControl.ReferenceScore.Value = referenceScore = new ScoreInfo
                {
                    HitEvents = TestSceneHitEventTimingDistributionGraph.CreateDistributedHitEvents(average_error),
                    BeatmapInfo = Beatmap.Value.BeatmapInfo,
                };
            });

            AddUntilStep("Has calibration button", () => offsetControl.ChildrenOfType<SettingsButton>().Any());
            AddStep("Press button", () => offsetControl.ChildrenOfType<SettingsButton>().Single().TriggerClick());
            AddAssert("Offset is adjusted", () => offsetControl.Current.Value == -average_error);

            AddUntilStep("Button is disabled", () => !offsetControl.ChildrenOfType<SettingsButton>().Single().Enabled.Value);
            AddStep("Remove reference score", () => offsetControl.ReferenceScore.Value = null);
            AddAssert("No calibration button", () => !offsetControl.ChildrenOfType<SettingsButton>().Any());

            recreateControl();
            AddStep("Set same reference score", () => offsetControl.ReferenceScore.Value = referenceScore);
            AddAssert("No calibration button", () => !offsetControl.ChildrenOfType<SettingsButton>().Any());
        }

        /// <summary>
        /// When a beatmap offset was already set, the calibration should take it into account.
        /// </summary>
        [Test]
        public void TestCalibrationFromNonZero()
        {
            const double average_error = -4.5;
            const double initial_offset = -2;

            AddStep("Set offset non-neutral", () => offsetControl.Current.Value = initial_offset);
            AddAssert("No calibration button", () => !offsetControl.ChildrenOfType<SettingsButton>().Any());
            AddStep("Set reference score", () =>
            {
                offsetControl.ReferenceScore.Value = new ScoreInfo
                {
                    HitEvents = TestSceneHitEventTimingDistributionGraph.CreateDistributedHitEvents(average_error),
                    BeatmapInfo = Beatmap.Value.BeatmapInfo,
                };
            });

            AddUntilStep("Has calibration button", () => offsetControl.ChildrenOfType<SettingsButton>().Any());
            AddStep("Press button", () => offsetControl.ChildrenOfType<SettingsButton>().Single().TriggerClick());
            AddAssert("Offset is adjusted", () => offsetControl.Current.Value == initial_offset - average_error);

            AddUntilStep("Button is disabled", () => !offsetControl.ChildrenOfType<SettingsButton>().Single().Enabled.Value);
            AddStep("Remove reference score", () => offsetControl.ReferenceScore.Value = null);
            AddAssert("No calibration button", () => !offsetControl.ChildrenOfType<SettingsButton>().Any());
        }

        [Test]
        public void TestCalibrationFromNonZeroWithImmediateReferenceScore()
        {
            const double average_error = -4.5;
            const double initial_offset = -2;

            AddStep("Set beatmap offset non-neutral", () => Realm.Write(r =>
            {
                r.Add(new BeatmapInfo
                {
                    ID = Beatmap.Value.BeatmapInfo.ID,
                    Ruleset = Beatmap.Value.BeatmapInfo.Ruleset,
                    UserSettings =
                    {
                        Offset = initial_offset,
                    }
                });
            }));

            AddStep("Create control with preloaded reference score", () =>
            {
                Child = new PlayerSettingsGroup("Some settings")
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        offsetControl = new BeatmapOffsetControl
                        {
                            ReferenceScore =
                            {
                                Value = new ScoreInfo
                                {
                                    HitEvents = TestSceneHitEventTimingDistributionGraph.CreateDistributedHitEvents(average_error),
                                    BeatmapInfo = Beatmap.Value.BeatmapInfo,
                                }
                            }
                        }
                    }
                };
            });

            AddUntilStep("Has calibration button", () => offsetControl.ChildrenOfType<SettingsButton>().Any());
            AddStep("Press button", () => offsetControl.ChildrenOfType<SettingsButton>().Single().TriggerClick());
            AddAssert("Offset is adjusted", () => offsetControl.Current.Value, () => Is.EqualTo(initial_offset - average_error));

            AddUntilStep("Button is disabled", () => !offsetControl.ChildrenOfType<SettingsButton>().Single().Enabled.Value);
            AddStep("Remove reference score", () => offsetControl.ReferenceScore.Value = null);
            AddAssert("No calibration button", () => !offsetControl.ChildrenOfType<SettingsButton>().Any());

            AddStep("Clean up beatmap", () => Realm.Write(r => r.RemoveAll<BeatmapInfo>()));
        }

        [Test]
        public void TestCalibrationNoChange()
        {
            const double average_error = 0;

            AddAssert("Offset is neutral", () => offsetControl.Current.Value == 0);
            AddAssert("No calibration button", () => !offsetControl.ChildrenOfType<SettingsButton>().Any());
            AddStep("Set reference score", () =>
            {
                offsetControl.ReferenceScore.Value = new ScoreInfo
                {
                    HitEvents = TestSceneHitEventTimingDistributionGraph.CreateDistributedHitEvents(average_error),
                    BeatmapInfo = Beatmap.Value.BeatmapInfo,
                };
            });

            AddUntilStep("Has calibration button", () => offsetControl.ChildrenOfType<SettingsButton>().Any());
            AddStep("Press button", () => offsetControl.ChildrenOfType<SettingsButton>().Single().TriggerClick());
            AddAssert("Offset is adjusted", () => offsetControl.Current.Value == -average_error);

            AddUntilStep("Button is disabled", () => !offsetControl.ChildrenOfType<SettingsButton>().Single().Enabled.Value);
            AddStep("Remove reference score", () => offsetControl.ReferenceScore.Value = null);
            AddAssert("No calibration button", () => !offsetControl.ChildrenOfType<SettingsButton>().Any());
        }

        private void recreateControl()
        {
            AddStep("Create control", () =>
            {
                Child = new PlayerSettingsGroup("Some settings")
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        offsetControl = new BeatmapOffsetControl()
                    }
                };
            });
        }
    }
}
