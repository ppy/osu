// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Storyboards;
using osu.Game.Storyboards.Drawables;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Storyboards
{
    [TestFixture]
    public class StoryboardSpriteLifetimeTest
    {
        private const string test_image_path = "Textures/test-image.png";

        [Test]
        public void TestOsu27627MultiFlashSpriteLifetime()
        {
            var sprite = decodeSingleSprite("osu27627-multi-flash-sprite.osb");

            // ppy/osu#27627: must not truncate at the first fade-out (55447); survive until the last flash ends.
            Assert.That(sprite.EndTimeForDisplay, Is.EqualTo(285_564));
            Assert.That(sprite.EndTimeForDisplay, Is.Not.EqualTo(55_447));
        }

        [Test]
        public void TestMultiFlashSpriteLifetimeIsNotTruncatedAtFirstFadeOut()
        {
            var sprite = decodeSingleSprite("multi-flash-sprite-lifetime.osb");

            // Minimal repro of ppy/osu#27627: repeated fade cycles must survive until the last visible alpha ends.
            Assert.Multiple(() =>
            {
                Assert.That(sprite.EndTime, Is.EqualTo(20_000));
                Assert.That(sprite.EndTimeForDisplay, Is.EqualTo(8_000));
            });
        }

        [Test]
        public void TestFadeStartValueExtendsDisplayEnd()
        {
            var sprite = decodeSingleSprite("fade-start-value-extends-display-end.osb");

            // ppy/osu#27753: a later fade with non-zero StartValue must not be ignored after an earlier fade-out.
            Assert.Multiple(() =>
            {
                Assert.That(sprite.EndTime, Is.EqualTo(15_000));
                Assert.That(sprite.EndTimeForDisplay, Is.EqualTo(6_000));
            });
        }

        [Test]
        public void TestTrailingInvisibleTransformsIgnoredForDisplayEnd()
        {
            var sprite = decodeSingleSprite("trailing-invisible-transforms-ignored-for-display-end.osb");

            Assert.Multiple(() =>
            {
                Assert.That(sprite.EndTime, Is.EqualTo(10_000));
                Assert.That(sprite.EndTimeForDisplay, Is.EqualTo(4_000));
            });
        }

        [Test]
        public void TestTrailingInvisibleLoopTransformsIgnoredForDisplayEnd()
        {
            var sprite = decodeSingleSprite("trailing-invisible-loop-transforms-ignored-for-display-end.osb");

            // L,2000,2 decodes to two loop iterations; last visible alpha ends at 4500 while trailing scale continues later.
            Assert.That(sprite.EndTimeForDisplay, Is.EqualTo(4_500));
        }

        [Test]
        public void TestDisplayEndFallsBackWhenNoVisibleAlphaExists()
        {
            var sprite = new StoryboardSprite(StoryboardElementSource.Beatmap, test_image_path, Anchor.Centre, Vector2.Zero);
            sprite.Commands.AddAlpha(Easing.None, 1000, 2000, 0, 0);
            sprite.Commands.AddScale(Easing.None, 2000, 5000, 1, 2);

            Assert.That(sprite.EndTimeForDisplay, Is.EqualTo(5_000));
        }

        [Test]
        public void TestVisibleAlphaEndRetainsConservativeEndTime()
        {
            var sprite = decodeSingleSprite("visible-alpha-end-retains-conservative-end-time.osb");

            // When the last alpha command still ends visible, do not truncate before trailing transforms.
            Assert.That(sprite.EndTimeForDisplay, Is.EqualTo(sprite.EndTime));
            Assert.That(sprite.EndTimeForDisplay, Is.EqualTo(10_000));
        }

        [Test]
        public void TestDrawableLifetimeEndUsesEndTimeForDisplay()
        {
            var sprite = new StoryboardSprite(StoryboardElementSource.Beatmap, test_image_path, Anchor.Centre, Vector2.Zero);
            sprite.Commands.AddAlpha(Easing.None, 1000, 2000, 0, 1);
            sprite.Commands.AddAlpha(Easing.None, 3000, 4000, 1, 0);
            sprite.Commands.AddScale(Easing.None, 4000, 10_000, 1, 2);

            var drawable = new DrawableStoryboardSprite(sprite);

            Assert.That(drawable.LifetimeEnd, Is.EqualTo(sprite.EndTimeForDisplay));
        }

        [Test]
        public void TestDrawableAnimationLifetimeEndUsesEndTimeForDisplay()
        {
            var animation = decodeSingleAnimation("animation-loop-no-explicit-end-time.osb");

            var drawable = new DrawableStoryboardAnimation(animation);

            Assert.That(drawable.LifetimeEnd, Is.EqualTo(animation.EndTimeForDisplay));
            Assert.That(drawable.LifetimeEnd, Is.EqualTo(12_000));
        }

        private static StoryboardSprite decodeSingleSprite(string resourceName)
        {
            var decoder = new LegacyStoryboardDecoder();

            using var resStream = TestResources.OpenResource(resourceName);
            using var stream = new LineBufferedReader(resStream);

            var storyboard = decoder.Decode(stream);
            var background = storyboard.Layers.Single(l => l.Depth == 3);

            Assert.That(background.Elements, Has.Count.EqualTo(1));

            return (StoryboardSprite)background.Elements.Single();
        }

        private static StoryboardAnimation decodeSingleAnimation(string resourceName)
        {
            var decoder = new LegacyStoryboardDecoder();

            using var resStream = TestResources.OpenResource(resourceName);
            using var stream = new LineBufferedReader(resStream);

            var storyboard = decoder.Decode(stream);
            var background = storyboard.Layers.Single(l => l.Depth == 3);

            Assert.That(background.Elements, Has.Count.EqualTo(1));

            return (StoryboardAnimation)background.Elements.Single();
        }
    }
}
