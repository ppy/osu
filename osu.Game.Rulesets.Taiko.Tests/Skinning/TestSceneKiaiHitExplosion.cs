// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    [TestFixture]
    public class TestSceneKiaiHitExplosion : TaikoSkinnableTestScene
    {
        [Test]
        public void TestKiaiHits()
        {
            AddStep("rim hit", () => SetContents(() => getContentFor(createHit(HitType.Rim))));
            AddStep("centre hit", () => SetContents(() => getContentFor(createHit(HitType.Centre))));
        }

        private Drawable getContentFor(DrawableTestHit hit)
        {
            return new Container
            {
                RelativeSizeAxes = Axes.Both,
                Child = new KiaiHitExplosion(hit, hit.HitObject.Type)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };
        }

        private DrawableTestHit createHit(HitType type) => new DrawableTestHit(new Hit { StartTime = Time.Current, Type = type });
    }
}
