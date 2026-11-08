// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Scoring;
using osu.Game.Screens.RankingV2.Legacy;

namespace osu.Game.Tests.Visual.RankingV2
{
    public partial class TestSceneLegacyRankingBackgroundOverlay : OsuTestScene
    {
        [Cached(typeof(IBindable<IScoreInfo>))]
        private readonly Bindable<IScoreInfo> score = new Bindable<IScoreInfo>();

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create component", () => Child = new Container
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    new LegacyRankingBackgroundOverlay(),
                    new SkinnableTestScene.OutlineBox(),
                ],
            });
        }
    }
}
