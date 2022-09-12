// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public class TestSceneSelectionBlueprintDeselection : TestSceneOsuEditor
    {
        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset, false);

        [Test]
        public void TestSingleDeleteAtSameTime()
        {
            HitCircle? circle1 = null;
            HitCircle? circle2 = null;

            AddStep("add two circles at the same time", () =>
            {
                circle1 = new HitCircle();
                circle2 = new HitCircle();
                EditorClock.Seek(0);
                EditorBeatmap.Add(circle1);
                EditorBeatmap.Add(circle2);
                EditorBeatmap.SelectedHitObjects.Add(circle1);
                EditorBeatmap.SelectedHitObjects.Add(circle2);
            });

            AddStep("delete the first circle", () => EditorBeatmap.Remove(circle1));
        }
    }
}
