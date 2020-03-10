// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestSceneHyperDash : PlayerTestScene
    {
        public TestSceneHyperDash()
            : base(new CatchRuleset())
        {
        }

        protected override bool Autoplay => true;

        [Test]
        public void TestHyperDash()
        {
            AddAssert("First note is hyperdash", () => Beatmap.Value.Beatmap.HitObjects[0] is Fruit f && f.HyperDash);
            AddUntilStep("wait for left hyperdash", () => getCatcher().Scale.X < 0 && getCatcher().HyperDashing);

            for (int i = 0; i < 2; i++)
            {
                AddUntilStep("wait for right hyperdash", () => getCatcher().Scale.X > 0 && getCatcher().HyperDashing);
                AddUntilStep("wait for left hyperdash", () => getCatcher().Scale.X < 0 && getCatcher().HyperDashing);
            }
        }

        private CatcherArea.Catcher getCatcher() => Player.ChildrenOfType<CatcherArea>().First().MovableCatcher;

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = new Beatmap
            {
                BeatmapInfo =
                {
                    Ruleset = ruleset,
                    BaseDifficulty = new BeatmapDifficulty { CircleSize = 3.6f }
                }
            };

            // Should produce a hyper-dash (edge case test)
            beatmap.HitObjects.Add(new Fruit { StartTime = 1816, X = 308 / 512f, NewCombo = true });
            beatmap.HitObjects.Add(new JuiceStream { StartTime = 2008, X = 56 / 512f, });

            double startTime = 3000;

            const float left_x = 0.02f;
            const float right_x = 0.98f;

            createObjects(() => new Fruit(), left_x);
            createObjects(() => new JuiceStream(), right_x);
            createObjects(() => new JuiceStream(), left_x);
            createObjects(() => new Fruit(), right_x);
            createObjects(() => new Fruit(), left_x);
            createObjects(() => new Fruit(), right_x);
            createObjects(() => new JuiceStream(), left_x);

            return beatmap;

            void createObjects(Func<CatchHitObject> createObject, float x)
            {
                const float spacing = 140;

                for (int i = 0; i < 3; i++)
                {
                    var hitObject = createObject();
                    hitObject.X = x;
                    hitObject.StartTime = startTime + i * spacing;

                    beatmap.HitObjects.Add(hitObject);
                }

                startTime += 700;
            }
        }
    }
}
