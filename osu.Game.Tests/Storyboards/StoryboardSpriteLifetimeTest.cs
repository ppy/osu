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
        public void TestDrawableLifetimeEndUsesEndTimeForDisplay()
        {
            var sprite = new StoryboardSprite(StoryboardElementSource.Beatmap, test_image_path, Anchor.Centre, Vector2.Zero);
            sprite.Commands.AddAlpha(Easing.None, 1000, 2000, 0, 1);
            sprite.Commands.AddAlpha(Easing.None, 3000, 4000, 1, 0);
            sprite.Commands.AddScale(Easing.None, 4000, 10_000, 1, 2);

            var drawable = new DrawableStoryboardSprite(sprite);

            Assert.That(drawable.LifetimeEnd, Is.EqualTo(sprite.EndTimeForDisplay));
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
    }
}
