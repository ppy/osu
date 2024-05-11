// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Rulesets.UI.Scrolling.Algorithms;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    /// <summary>
    /// A test scene for skinnable mania components.
    /// </summary>
    public abstract partial class ManiaSkinnableTestScene : SkinnableTestScene
    {
        [Cached(Type = typeof(IScrollingInfo))]
        private readonly TestScrollingInfo scrollingInfo = new TestScrollingInfo();

        [Cached]
        private readonly StageDefinition stage = new StageDefinition(4);

        protected override Ruleset CreateRulesetForSkinProvider() => new ManiaRuleset();

        protected ManiaSkinnableTestScene()
        {
            scrollingInfo.Direction.Value = ScrollingDirection.Down;

            Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.SlateGray.Opacity(0.2f),
                Depth = 1
            });
        }

        [Test]
        public void TestScrollingDown()
        {
            AddStep("change direction to down", () => scrollingInfo.Direction.Value = ScrollingDirection.Down);
        }

        [Test]
        public void TestScrollingUp()
        {
            AddStep("change direction to up", () => scrollingInfo.Direction.Value = ScrollingDirection.Up);
        }

        private class TestScrollingInfo : IScrollingInfo
        {
            public readonly Bindable<ScrollingDirection> Direction = new Bindable<ScrollingDirection>();

            IBindable<ScrollingDirection> IScrollingInfo.Direction => Direction;
            IBindable<double> IScrollingInfo.TimeRange { get; } = new Bindable<double>(5000);
            IBindable<IScrollAlgorithm> IScrollingInfo.Algorithm { get; } = new Bindable<IScrollAlgorithm>(new ConstantScrollAlgorithm());
        }
    }
}
