// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Gameplay
{
    [HeadlessTest]
    public partial class TestSceneProxyContainer : OsuTestScene
    {
        private HitObjectContainer hitObjectContainer;
        private ProxyContainer proxyContainer;
        private readonly ManualClock clock = new ManualClock();

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = new Container
            {
                Children = new Drawable[]
                {
                    hitObjectContainer = new HitObjectContainer(),
                    proxyContainer = new ProxyContainer()
                },
                Clock = new FramedClock(clock)
            };
            clock.CurrentTime = 0;
        });

        [Test]
        public void TestProxyLifetimeManagement()
        {
            AddStep("Add proxy drawables", () =>
            {
                addProxy(new TestDrawableHitObject(1000));
                addProxy(new TestDrawableHitObject(3000));
                addProxy(new TestDrawableHitObject(5000));
            });

            AddStep("time = 1000", () => clock.CurrentTime = 1000);
            AddAssert("One proxy is alive", () => proxyContainer.AliveChildren.Count == 1);
            AddStep("time = 5000", () => clock.CurrentTime = 5000);
            AddAssert("One proxy is alive", () => proxyContainer.AliveChildren.Count == 1);
            AddStep("time = 6000", () => clock.CurrentTime = 6000);
            AddAssert("No proxy is alive", () => proxyContainer.AliveChildren.Count == 0);
        }

        private void addProxy(DrawableHitObject drawableHitObject)
        {
            hitObjectContainer.Add(drawableHitObject);
            proxyContainer.AddProxy(drawableHitObject);
        }

        private partial class ProxyContainer : LifetimeManagementContainer
        {
            public IReadOnlyList<Drawable> AliveChildren => AliveInternalChildren;

            public void AddProxy(Drawable d) => AddInternal(d.CreateProxy());
        }

        private partial class TestDrawableHitObject : DrawableHitObject
        {
            protected override double InitialLifetimeOffset => 100;

            public TestDrawableHitObject(double startTime)
                : base(new HitObject { StartTime = startTime })
            {
            }

            protected override void UpdateInitialTransforms()
            {
                LifetimeEnd = LifetimeStart + 500;
            }
        }
    }
}
