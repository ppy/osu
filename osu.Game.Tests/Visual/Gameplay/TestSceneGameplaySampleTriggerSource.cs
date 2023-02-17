// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Timing;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Storyboards;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneGameplaySampleTriggerSource : PlayerTestScene
    {
        private TestGameplaySampleTriggerSource sampleTriggerSource = null!;
        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        private Beatmap beatmap = null!;

        [Resolved]
        private AudioManager audio { get; set; } = null!;

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard? storyboard = null)
            => new ClockBackedTestWorkingBeatmap(beatmap, storyboard, new FramedClock(new ManualClock { Rate = 1 }), audio);

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            beatmap = new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty { CircleSize = 6, SliderMultiplier = 3 },
                    Ruleset = ruleset
                }
            };

            const double start_offset = 8000;
            const double spacing = 2000;

            // intentionally start objects a bit late so we can test the case of no alive objects.
            double t = start_offset;

            beatmap.HitObjects.AddRange(new[]
            {
                new HitCircle
                {
                    StartTime = t += spacing,
                    Samples = new[] { new HitSampleInfo(HitSampleInfo.HIT_NORMAL) }
                },
                new HitCircle
                {
                    StartTime = t += spacing,
                    Samples = new[] { new HitSampleInfo(HitSampleInfo.HIT_WHISTLE) }
                },
                new HitCircle
                {
                    StartTime = t += spacing,
                    Samples = new[] { new HitSampleInfo(HitSampleInfo.HIT_NORMAL) },
                    SampleControlPoint = new SampleControlPoint { SampleBank = "soft" },
                },
                new HitCircle
                {
                    StartTime = t + spacing,
                    Samples = new[] { new HitSampleInfo(HitSampleInfo.HIT_WHISTLE) },
                    SampleControlPoint = new SampleControlPoint { SampleBank = "soft" },
                },
            });

            return beatmap;
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("Add trigger source", () => Player.HUDOverlay.Add(sampleTriggerSource = new TestGameplaySampleTriggerSource(Player.DrawableRuleset.Playfield.HitObjectContainer)));
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

            checkValidObjectIndex(1);

            // Still object 1 as it's not hit yet.
            seekBeforeIndex(1);
            waitForAliveObjectIndex(1);
            checkValidObjectIndex(1);

            seekBeforeIndex(2);
            waitForAliveObjectIndex(2);
            checkValidObjectIndex(2);

            seekBeforeIndex(3);
            waitForAliveObjectIndex(3);
            checkValidObjectIndex(3);

            AddStep("Seek into future", () => Beatmap.Value.Track.Seek(beatmap.HitObjects.Last().GetEndTime() + 10000));

            waitForAliveObjectIndex(null);
            checkValidObjectIndex(3);
        }

        private void seekBeforeIndex(int index) =>
            AddStep($"seek to just before object {index}", () => Beatmap.Value.Track.Seek(beatmap.HitObjects[index].StartTime - 100));

        private void waitForAliveObjectIndex(int? index)
        {
            if (index == null)
                AddUntilStep("wait for no alive objects", getNextAliveObject, () => Is.Null);
            else
                AddUntilStep($"wait for next alive to be {index}", () => getNextAliveObject()?.HitObject, () => Is.EqualTo(beatmap.HitObjects[index.Value]));
        }

        private void checkValidObjectIndex(int index) =>
            AddAssert($"check valid object is {index}", () => sampleTriggerSource.GetMostValidObject(), () => Is.EqualTo(beatmap.HitObjects[index]));

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

            public new HitObject GetMostValidObject() => base.GetMostValidObject();
        }
    }
}
