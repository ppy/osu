// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    public class TestSceneTaikoScroller : TaikoSkinnableTestScene
    {
        public TestSceneTaikoScroller()
        {
            AddStep("Load scroller", () => SetContents(() => new SkinnableDrawable(new TaikoSkinComponent(TaikoSkinComponents.TaikoScroller), _ => Drawable.Empty())));
        }
    }
}
