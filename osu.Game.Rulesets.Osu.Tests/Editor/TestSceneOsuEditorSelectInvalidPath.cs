// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public partial class TestSceneOsuEditorSelectInvalidPath : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset, false);

        [Test]
        public void TestSelectDoesNotModify()
        {
            Slider slider = new Slider { StartTime = 0, Position = new Vector2(320, 40) };

            PathControlPoint[] points =
            {
                new PathControlPoint(new Vector2(0), PathType.PERFECT_CURVE),
                new PathControlPoint(new Vector2(-100, 0)),
                new PathControlPoint(new Vector2(100, 20))
            };

            int preSelectVersion = -1;
            AddStep("add slider", () =>
            {
                slider.Path = new SliderPath(points);
                EditorBeatmap.Add(slider);
                preSelectVersion = slider.Path.Version.Value;
            });

            AddStep("select added slider", () => EditorBeatmap.SelectedHitObjects.Add(slider));

            AddAssert("slider same path", () => slider.Path.Version.Value == preSelectVersion);
        }
    }
}
