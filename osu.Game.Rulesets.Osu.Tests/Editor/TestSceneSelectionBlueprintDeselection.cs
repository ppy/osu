// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Beatmaps;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public class TestSceneSelectionBlueprintDeselection : TestSceneOsuEditor
    {
        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset, false);

        [Test]
        public void TestSingleDeleteAtSameTime()
        {
            HitCircle? circle1 = null;

            AddStep("add two circles at the same time", () =>
            {
                EditorClock.Seek(0);
                circle1 = new HitCircle();
                var circle2 = new HitCircle();
                EditorBeatmap.Add(circle1);
                EditorBeatmap.Add(circle2);
                EditorBeatmap.SelectedHitObjects.Add(circle1);
                EditorBeatmap.SelectedHitObjects.Add(circle2);
            });

            AddStep("delete the first circle", () => EditorBeatmap.Remove(circle1));
        }

        [Test]
        public void TestBigStackDeleteAtSameTime()
        {
            AddStep("add 20 circles at the same time", () =>
            {
                EditorClock.Seek(0);

                for (int i = 0; i < 20; i++)
                {
                    EditorBeatmap.Add(new HitCircle());
                }
            });

            AddStep("select half of the circles", () =>
            {
                foreach (var hitObject in EditorBeatmap.HitObjects.SkipLast(10).Reverse())
                {
                    EditorBeatmap.SelectedHitObjects.Add(hitObject);
                }
            });

            AddStep("delete all selected circles", () =>
            {
                InputManager.PressKey(Key.Delete);
                InputManager.ReleaseKey(Key.Delete);
            });
        }
    }
}
