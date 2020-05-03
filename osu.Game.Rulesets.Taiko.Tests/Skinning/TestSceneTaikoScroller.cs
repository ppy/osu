// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Testing;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Skinning;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    public class TestSceneTaikoScroller : TaikoSkinnableTestScene
    {
        public TestSceneTaikoScroller()
        {
            AddStep("Load scroller", () => SetContents(() => new SkinnableDrawable(new TaikoSkinComponent(TaikoSkinComponents.TaikoScroller), _ => Empty())));
            AddToggleStep("Toggle passing", passing => this.ChildrenOfType<LegacyTaikoScroller>().ForEach(s => s.LastResult.Value =
                new JudgementResult(null, new Judgement()) { Type = passing ? HitResult.Perfect : HitResult.Miss }));
        }
    }
}
