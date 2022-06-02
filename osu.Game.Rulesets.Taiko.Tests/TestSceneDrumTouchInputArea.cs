// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;

namespace osu.Game.Rulesets.Taiko.Tests
{
    [TestFixture]
    public class TestSceneDrumTouchInputArea : DrawableTaikoRulesetTestScene
    {
        protected const double NUM_HIT_OBJECTS = 10;
        protected const double HIT_OBJECT_TIME_SPACING_MS = 1000;

        [BackgroundDependencyLoader]
        private void load()
        {
            var drumTouchInputArea = new DrumTouchInputArea();
            DrawableRuleset.KeyBindingInputManager.Add(drumTouchInputArea);
            drumTouchInputArea.ShowTouchControls();
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            List<TaikoHitObject> hitObjects = new List<TaikoHitObject>();

            for (int i = 0; i < NUM_HIT_OBJECTS; i++)
            {
                hitObjects.Add(new Hit
                {
                    StartTime = Time.Current + i * HIT_OBJECT_TIME_SPACING_MS,
                    IsStrong = isOdd(i),
                    Type = isOdd(i / 2) ? HitType.Centre : HitType.Rim
                });
            }

            var beatmap = new Beatmap<TaikoHitObject>
            {
                BeatmapInfo = { Ruleset = ruleset },
                HitObjects = hitObjects
            };

            return beatmap;
        }

        private bool isOdd(int number)
        {
            return number % 2 == 0;
        }
    }
}
