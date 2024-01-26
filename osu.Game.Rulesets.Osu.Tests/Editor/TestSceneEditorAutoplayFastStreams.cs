// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    /// <summary>
    /// This test covers autoplay working correctly in the editor on fast streams.
    /// Might seem like a weird test, but frame stability being toggled can cause autoplay to operation incorrectly.
    /// This is clearly a bug with the autoplay algorithm, but is worked around at an editor level for now.
    /// </summary>
    public partial class TestSceneEditorAutoplayFastStreams : EditorTestScene
    {
        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var testBeatmap = new TestBeatmap(ruleset, false);
            testBeatmap.HitObjects.AddRange(new[]
            {
                new HitCircle { StartTime = 500 },
                new HitCircle { StartTime = 530 },
                new HitCircle { StartTime = 560 },
                new HitCircle { StartTime = 590 },
                new HitCircle { StartTime = 620 },
            });

            return testBeatmap;
        }

        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        [Test]
        public void TestAllHit()
        {
            AddStep("start playback", () => EditorClock.Start());
            AddUntilStep("wait for all hit", () =>
            {
                DrawableHitCircle[] hitCircles = Editor.ChildrenOfType<DrawableHitCircle>().OrderBy(s => s.HitObject.StartTime).ToArray();

                return hitCircles.Length == 5 && hitCircles.All(h => h.IsHit);
            });
        }
    }
}
