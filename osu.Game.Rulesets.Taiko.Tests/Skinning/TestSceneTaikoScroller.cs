// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Skinning.Legacy;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    public partial class TestSceneTaikoScroller : TaikoSkinnableTestScene
    {
        private readonly ManualClock clock = new ManualClock();

        private bool reversed;

        public TestSceneTaikoScroller()
        {
            AddStep("Load scroller", () => SetContents(_ =>
                new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.Scroller), _ => Empty())
                {
                    Clock = new FramedClock(clock),
                    Height = 0.4f,
                }));

            AddToggleStep("Toggle passing", passing => this.ChildrenOfType<LegacyTaikoScroller>().ForEach(s => s.LastResult.Value =
                new Judgement(new HitObject(), new JudgementInfo()) { Type = passing ? HitResult.Great : HitResult.Miss }));

            AddToggleStep("toggle playback direction", reversed => this.reversed = reversed);
        }

        protected override void Update()
        {
            base.Update();

            clock.CurrentTime += (reversed ? -1 : 1) * Clock.ElapsedFrameTime;
        }
    }
}
