// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Storyboards;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneGameplaySampleTriggerSource : PlayerTestScene
    {
        protected override bool AllowBackwardsSeeks => true;

        private TestGameplaySampleTriggerSource sampleTriggerSource = null!;
        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        private Beatmap beatmap = null!;

        [Resolved]
        private AudioManager audio { get; set; } = null!;

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard? storyboard = null)
            => new ClockBackedTestWorkingBeatmap(beatmap, storyboard, new FramedClock(new ManualClock { Rate = 1 }), audio);

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            ControlPointInfo controlPointInfo = new LegacyControlPointInfo();

            beatmap = new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty { CircleSize = 6, SliderMultiplier = 3 },
                    Ruleset = ruleset
                },
                ControlPointInfo = controlPointInfo
            };

            const double start_offset = 8000;
            const double spacing = 2000;

            // intentionally start objects a bit late so we can test the case of no alive objects.
            double t = start_offset;

            beatmap.HitObjects.AddRange(new HitObject[]
            {
                new HitCircle
                {
                    HitWindows = new HitWindows(),
                    StartTime = t += spacing,
                    Samples = new[] { new HitSampleInfo(HitSampleInfo.HIT_NORMAL) }
                },
                new HitCircle
                {
                    HitWindows = new HitWindows(),
                    StartTime = t += spacing,
                    Samples = new[] { new HitSampleInfo(HitSampleInfo.HIT_WHISTLE) }
                },
                new HitCircle
                {
                    HitWindows = new HitWindows(),
                    StartTime = t += spacing,
                    Samples = new[] { new HitSampleInfo(HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_SOFT) },
                },
                new HitCircle
                {
                    HitWindows = new HitWindows(),
                    StartTime = t += spacing,
                },
                new Slider
                {
                    HitWindows = new HitWindows(),
                    StartTime = t += spacing,
                    Path = new SliderPath(PathType.LINEAR, new[] { Vector2.Zero, Vector2.UnitY * 200 }),
                    Samples = new[] { new HitSampleInfo(HitSampleInfo.HIT_WHISTLE, HitSampleInfo.BANK_SOFT) },
                },
            });

            // Add a change in volume halfway through final slider.
            controlPointInfo.Add(t, new SampleControlPoint
            {
                SampleBank = "normal",
                SampleVolume = 20,
            });

            return beatmap;
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("Add trigger source", () => Player.DrawableRuleset.FrameStableComponents.Add(sampleTriggerSource = new TestGameplaySampleTriggerSource(Player.DrawableRuleset.Playfield.HitObjectContainer)));
        }

        [Test]
        public void TestCorrectHitObject()
        {
            waitForAliveObjectIndex(null);
            checkValidObjectIndex(0);

            seekBeforeIndex(0);
            waitForAliveObjectIndex(0);
            checkValidObjectIndex(0);

            AddAssert("first object not hit", () => getNextAliveObject()?.Entry?.Result?.HasResult != true);

            AddStep("hit first object", () =>
            {
                var next = getNextAliveObject();

                if (next != null)
                {
                    Debug.Assert(next.Entry?.Result?.HasResult != true);

                    InputManager.MoveMouseTo(next.ScreenSpaceDrawQuad.Centre);
                    InputManager.Click(MouseButton.Left);
                }
            });

            AddAssert("first object hit", () => getNextAliveObject()?.Entry?.Result?.HasResult == true);

            // next object is too far away, so we still use the already hit object.
            checkValidObjectIndex(0);

            // still too far away.
            seekBeforeIndex(1, 400);
            checkValidObjectIndex(0);

            // Still object 1 as it's not hit yet.
            seekBeforeIndex(1);
            waitForAliveObjectIndex(1);
            checkValidObjectIndex(1);

            seekBeforeIndex(2);
            waitForAliveObjectIndex(2);
            checkValidObjectIndex(2);

            // test rewinding
            seekBeforeIndex(1);
            waitForAliveObjectIndex(1);
            checkValidObjectIndex(1);

            seekBeforeIndex(1, 400);
            checkValidObjectIndex(0);

            seekBeforeIndex(3);
            waitForAliveObjectIndex(3);
            checkValidObjectIndex(3);

            seekBeforeIndex(4);
            waitForAliveObjectIndex(4);

            // Even before the object, we should prefer the first nested object's sample.
            // This is because the (parent) object will only play its sample at the final EndTime.
            AddAssert("check valid object is slider's first nested", () => sampleTriggerSource.GetMostValidObject(), () => Is.EqualTo(beatmap.HitObjects[4].NestedHitObjects.First()));

            AddStep("seek to just before slider ends", () => Player.GameplayClockContainer.Seek(beatmap.HitObjects[4].GetEndTime() - 100));
            waitForCatchUp();
            AddUntilStep("wait until valid object is slider's last nested", () => sampleTriggerSource.GetMostValidObject(), () => Is.EqualTo(beatmap.HitObjects[4].NestedHitObjects.Last()));

            // After we get far enough away, the samples of the object itself should be used, not any nested object.
            AddStep("seek to further after slider", () => Player.GameplayClockContainer.Seek(beatmap.HitObjects[4].GetEndTime() + 1000));
            waitForCatchUp();
            AddUntilStep("wait until valid object is slider itself", () => sampleTriggerSource.GetMostValidObject(), () => Is.EqualTo(beatmap.HitObjects[4]));

            AddStep("Seek into future", () => Player.GameplayClockContainer.Seek(beatmap.HitObjects.Last().GetEndTime() + 10000));
            waitForCatchUp();
            waitForAliveObjectIndex(null);
            checkValidObjectIndex(4);
        }

        private void seekBeforeIndex(int index, double amount = 100)
        {
            AddStep($"seek to {amount} ms before object {index}", () => Player.GameplayClockContainer.Seek(beatmap.HitObjects[index].StartTime - amount));
            waitForCatchUp();
        }

        private void waitForCatchUp() =>
            AddUntilStep("wait for frame stable clock to catch up", () => Precision.AlmostEquals(Player.GameplayClockContainer.CurrentTime, Player.DrawableRuleset.FrameStableClock.CurrentTime));

        private void waitForAliveObjectIndex(int? index)
        {
            if (index == null)
                AddUntilStep("wait for no alive objects", getNextAliveObject, () => Is.Null);
            else
                AddUntilStep($"wait for next alive to be {index}", () => getNextAliveObject()?.HitObject, () => Is.EqualTo(beatmap.HitObjects[index.Value]));
        }

        private void checkValidObjectIndex(int index) =>
            AddAssert($"check object at index {index} is correct", () => sampleTriggerSource.GetMostValidObject(), () => Is.EqualTo(beatmap.HitObjects[index]));

        private DrawableHitObject? getNextAliveObject() =>
            Player.DrawableRuleset.Playfield.HitObjectContainer.AliveObjects.FirstOrDefault();

        [Test]
        public void TestSampleTriggering()
        {
            AddRepeatStep("trigger sample", () => sampleTriggerSource.Play(), 10);
        }

        public partial class TestGameplaySampleTriggerSource : GameplaySampleTriggerSource
        {
            public TestGameplaySampleTriggerSource(HitObjectContainer hitObjectContainer)
                : base(hitObjectContainer)
            {
            }

            public new HitObject? GetMostValidObject() => base.GetMostValidObject();
        }
    }
}
