// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Beatmaps;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneSelectionBlueprintDeselection : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

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
            AddAssert("one hitobject remains", () => EditorBeatmap.HitObjects.Count == 1);
            AddAssert("one hitobject selected", () => EditorBeatmap.SelectedHitObjects.Count == 1);
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

            AddAssert("10 hitobjects remain", () => EditorBeatmap.HitObjects.Count == 10);
            AddAssert("no hitobjects selected", () => EditorBeatmap.SelectedHitObjects.Count == 0);
        }
    }
}
