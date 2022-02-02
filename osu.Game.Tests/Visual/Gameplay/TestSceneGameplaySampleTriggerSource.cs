// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.UI;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneGameplaySampleTriggerSource : PlayerTestScene
    {
        private TestGameplaySampleTriggerSource sampleTriggerSource;
        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        private Beatmap beatmap;

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

            double t = start_offset;
            beatmap.HitObjects.AddRange(new[]
            {
                new HitCircle
                {
                    // intentionally start objects a bit late so we can test the case of no alive objects.
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
            HitObjectLifetimeEntry nextObjectEntry = null;

            AddAssert("no alive objects", () => getNextAliveObject() == null);

            AddAssert("check initially correct object", () => sampleTriggerSource.GetMostValidObject() == beatmap.HitObjects[0]);

            AddUntilStep("get next object", () =>
            {
                var nextDrawableObject = getNextAliveObject();

                if (nextDrawableObject != null)
                {
                    nextObjectEntry = nextDrawableObject.Entry;
                    InputManager.MoveMouseTo(nextDrawableObject.ScreenSpaceDrawQuad.Centre);
                    return true;
                }

                return false;
            });

            AddUntilStep("hit first hitobject", () =>
            {
                InputManager.Click(MouseButton.Left);
                return nextObjectEntry.Result.HasResult;
            });

            AddAssert("check correct object after hit", () => sampleTriggerSource.GetMostValidObject() == beatmap.HitObjects[1]);

            AddUntilStep("check correct object after miss", () => sampleTriggerSource.GetMostValidObject() == beatmap.HitObjects[2]);
            AddUntilStep("check correct object after miss", () => sampleTriggerSource.GetMostValidObject() == beatmap.HitObjects[3]);

            AddUntilStep("no alive objects", () => getNextAliveObject() == null);
            AddAssert("check correct object after none alive", () => sampleTriggerSource.GetMostValidObject() == beatmap.HitObjects[3]);
        }

        private DrawableHitObject getNextAliveObject() =>
            Player.DrawableRuleset.Playfield.HitObjectContainer.AliveObjects.FirstOrDefault();

        [Test]
        public void TestSampleTriggering()
        {
            AddRepeatStep("trigger sample", () => sampleTriggerSource.Play(), 10);
        }

        public class TestGameplaySampleTriggerSource : GameplaySampleTriggerSource
        {
            public TestGameplaySampleTriggerSource(HitObjectContainer hitObjectContainer)
                : base(hitObjectContainer)
            {
            }

            public new HitObject GetMostValidObject() => base.GetMostValidObject();
        }
    }
}
