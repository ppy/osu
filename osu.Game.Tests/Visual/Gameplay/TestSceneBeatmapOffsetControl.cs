// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
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
        private OsuConfigManager localConfig = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(localConfig = new OsuConfigManager(LocalStorage));
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("reset settings", () => localConfig.SetValue(OsuSetting.AutomaticallyAdjustBeatmapOffset, false));

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

        /// <summary>
        /// If we already have an old score with enough hit events and the new score doesn't have enough, continue displaying the old one rather than showing the user "play too short" message.
        /// </summary>
        [Test]
        public void TestTooShortToDisplay_HasPreviousValidScore()
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

            AddStep("Set short reference score", () =>
            {
                offsetControl.ReferenceScore.Value = new ScoreInfo
                {
                    HitEvents = TestSceneHitEventTimingDistributionGraph.CreateDistributedHitEvents(0, 2),
                    BeatmapInfo = Beatmap.Value.BeatmapInfo,
                };
            });

            AddUntilStep("Still calibration button", () => offsetControl.ChildrenOfType<SettingsButton>().Any());

            AddStep("Press button", () => offsetControl.ChildrenOfType<SettingsButton>().Single().TriggerClick());
            AddAssert("Offset is adjusted", () => offsetControl.Current.Value == initial_offset - average_error);
        }

        [Test]
        public void TestNotEnoughTimedHitEvents()
        {
            AddStep("Set short reference score", () =>
            {
                // 50 events total. one of them (head circle) being timed / having hitwindows, rest having no hitwindows
                List<HitEvent> hitEvents =
                [
                    new HitEvent(30, 1, HitResult.LargeTickHit, new SliderHeadCircle { ClassicSliderBehaviour = true }, null, null),
                ];

                for (int i = 0; i < 49; i++)
                {
                    hitEvents.Add(new HitEvent(0, 1, HitResult.LargeTickHit, new SliderTick(), null, null));
                }

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
            AddAssert("Offset is still neutral", () => offsetControl.Current.Value == 0);
            AddStep("Press button", () => offsetControl.ChildrenOfType<SettingsButton>().Single().TriggerClick());
            AddAssert("Offset is adjusted", () => offsetControl.Current.Value == -average_error);
            AddUntilStep("Button is disabled", () => !offsetControl.ChildrenOfType<SettingsButton>().Single().Enabled.Value);

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
            ScoreInfo referenceScore = null!;
            const double average_error = -4.5;
            const double initial_offset = -2;

            AddStep("Set offset non-neutral", () => offsetControl.Current.Value = initial_offset);
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
            AddAssert("Offset still not adjusted", () => offsetControl.Current.Value == initial_offset);
            AddStep("Press button", () => offsetControl.ChildrenOfType<SettingsButton>().Single().TriggerClick());
            AddAssert("Offset is adjusted", () => offsetControl.Current.Value == initial_offset - average_error);
            AddUntilStep("Button is disabled", () => !offsetControl.ChildrenOfType<SettingsButton>().Single().Enabled.Value);

            recreateControl();
            AddStep("Set same reference score", () => offsetControl.ReferenceScore.Value = referenceScore);
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
            AddAssert("Offset still not adjusted", () => offsetControl.Current.Value == initial_offset);
            AddStep("Press button", () => offsetControl.ChildrenOfType<SettingsButton>().Single().TriggerClick());
            AddAssert("Offset is adjusted", () => offsetControl.Current.Value, () => Is.EqualTo(initial_offset - average_error));
            AddUntilStep("Button is disabled", () => !offsetControl.ChildrenOfType<SettingsButton>().Single().Enabled.Value);

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
        }

        [Test]
        public void TestAutomaticAdjustment()
        {
            const double average_error = -4.5;

            AddStep("enable automatic adjust", () => localConfig.SetValue(OsuSetting.AutomaticallyAdjustBeatmapOffset, true));
            AddAssert("offset zero", () => offsetControl.Current.Value == 0);

            AddStep("set reference score", () =>
            {
                offsetControl.ReferenceScore.Value = new ScoreInfo
                {
                    HitEvents = TestSceneHitEventTimingDistributionGraph.CreateDistributedHitEvents(average_error),
                    BeatmapInfo = Beatmap.Value.BeatmapInfo,
                };
            });

            AddAssert("no calibration button", () => !offsetControl.ChildrenOfType<SettingsButton>().Any(b => b.IsPresent));
            AddAssert("offset adjustment text displayed", () => offsetControl.ChildrenOfType<IHasText>().Any(t => t.Text.ToString().Contains("adjusted")));
            AddAssert("offset adjusted", () => offsetControl.Current.Value == -average_error);

            AddStep("set reference score", () =>
            {
                offsetControl.ReferenceScore.Value = new ScoreInfo
                {
                    HitEvents = TestSceneHitEventTimingDistributionGraph.CreateDistributedHitEvents(0),
                    BeatmapInfo = Beatmap.Value.BeatmapInfo,
                };
            });

            AddAssert("no calibration button", () => !offsetControl.ChildrenOfType<SettingsButton>().Any(b => b.IsPresent));
            AddAssert("offset adjustment text not displayed", () => !offsetControl.ChildrenOfType<IHasText>().Any(t => t.Text.ToString().Contains("adjusted")));
            AddAssert("offset still", () => offsetControl.Current.Value == -average_error);

            AddStep("adjust offset manually", () => offsetControl.Current.Value = 0);
            AddUntilStep("calibration button displayed", () => offsetControl.ChildrenOfType<SettingsButton>().Any());

            AddStep("press button", () => offsetControl.ChildrenOfType<SettingsButton>().Single().TriggerClick());
            AddAssert("offset adjusted", () => offsetControl.Current.Value == -average_error);
            AddUntilStep("button is disabled", () => !offsetControl.ChildrenOfType<SettingsButton>().Single().Enabled.Value);
        }

        [Test]
        public void TestAutomaticAdjustmentWithUnstableRate()
        {
            const double average_error = -25;
            const int spread = 25;
            const double expected_offset = 12.9; // due to high UR (~147). see BeatmapOffsetControl.computeSuggestedOffset()

            AddStep("enable automatic adjust", () => localConfig.SetValue(OsuSetting.AutomaticallyAdjustBeatmapOffset, true));
            AddAssert("offset zero", () => offsetControl.Current.Value == 0);

            AddStep("set reference score", () =>
            {
                offsetControl.ReferenceScore.Value = new ScoreInfo
                {
                    // distribute the hit events such that it produces ~147 UR. setup taken from UnstableRateTest.
                    HitEvents = Enumerable.Range((int)average_error - spread, spread * 2 + 1)
                                          .Select(t => new HitEvent(t, 1.0, HitResult.Great, new HitObject(), null, null))
                                          .ToList(),

                    BeatmapInfo = Beatmap.Value.BeatmapInfo,
                };
            });

            AddAssert("no calibration button", () => !offsetControl.ChildrenOfType<SettingsButton>().Any(b => b.IsPresent));
            AddAssert("offset adjustment text displayed", () => offsetControl.ChildrenOfType<IHasText>().Any(t => t.Text.ToString().Contains("adjusted")));
            AddAssert("offset adjusted", () => offsetControl.Current.Value == expected_offset);

            AddStep("adjust offset manually", () => offsetControl.Current.Value = 0);
            AddUntilStep("calibration button displayed", () => offsetControl.ChildrenOfType<SettingsButton>().Any());

            AddStep("press button", () => offsetControl.ChildrenOfType<SettingsButton>().Single().TriggerClick());
            AddAssert("offset adjusted", () => offsetControl.Current.Value == expected_offset);
            AddUntilStep("button is disabled", () => !offsetControl.ChildrenOfType<SettingsButton>().Single().Enabled.Value);
        }

        [Test]
        public void TestNegativeZero()
        {
            AddAssert("assert", () => BeatmapOffsetControl.GetOffsetExplanatoryText(-0.0001).ToString(), () => Is.EqualTo("0 ms"));
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
