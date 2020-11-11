// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    [TestFixture]
    public class TestSceneHitExplosion : TaikoSkinnableTestScene
    {
        [Test]
        public void TestNormalHit()
        {
            AddStep("Great", () => SetContents(() => getContentFor(createHit(HitResult.Great))));
            AddStep("Ok", () => SetContents(() => getContentFor(createHit(HitResult.Ok))));
            AddStep("Miss", () => SetContents(() => getContentFor(createHit(HitResult.Miss))));
        }

        [Test]
        public void TestStrongHit([Values(false, true)] bool hitBoth)
        {
            AddStep("Great", () => SetContents(() => getContentFor(createStrongHit(HitResult.Great, hitBoth))));
            AddStep("Good", () => SetContents(() => getContentFor(createStrongHit(HitResult.Ok, hitBoth))));
        }

        private Drawable getContentFor(DrawableTestHit hit)
        {
            return new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    // the hit needs to be added to hierarchy in order for nested objects to be created correctly.
                    // setting zero alpha is supposed to prevent the test from looking broken.
                    hit.With(h => h.Alpha = 0),
                    new HitExplosion(hit, hit.Type)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                }
            };
        }

        private DrawableTestHit createHit(HitResult type) => new DrawableTestHit(new Hit { StartTime = Time.Current }, type);

        private DrawableTestHit createStrongHit(HitResult type, bool hitBoth) => new DrawableTestStrongHit(Time.Current, type, hitBoth);
    }
}
