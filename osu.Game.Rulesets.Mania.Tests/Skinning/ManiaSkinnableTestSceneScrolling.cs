// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Rulesets.UI.Scrolling.Algorithms;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    /// <summary>
    /// A test scene for skinnable mania components, with an <see cref="IScrollingInfo"/> provided.
    /// </summary>
    public abstract partial class ManiaSkinnableTestSceneScrolling : ManiaSkinnableTestScene
    {
        [Cached(Type = typeof(IScrollingInfo))]
        protected readonly TestScrollingInfo ScrollingInfo = new TestScrollingInfo();

        protected ManiaSkinnableTestSceneScrolling()
        {
            ScrollingInfo.Direction.Value = ScrollingDirection.Down;
        }

        [Test]
        public void TestScrollingDown()
        {
            AddStep("change direction to down", () => ScrollingInfo.Direction.Value = ScrollingDirection.Down);
        }

        [Test]
        public void TestScrollingUp()
        {
            AddStep("change direction to up", () => ScrollingInfo.Direction.Value = ScrollingDirection.Up);
        }

        protected class TestScrollingInfo : IScrollingInfo
        {
            public readonly Bindable<ScrollingDirection> Direction = new Bindable<ScrollingDirection>();

            IBindable<ScrollingDirection> IScrollingInfo.Direction => Direction;
            IBindable<double> IScrollingInfo.TimeRange { get; } = new Bindable<double>(5000);
            IBindable<IScrollAlgorithm> IScrollingInfo.Algorithm { get; } = new Bindable<IScrollAlgorithm>(new ConstantScrollAlgorithm());
        }
    }
}
