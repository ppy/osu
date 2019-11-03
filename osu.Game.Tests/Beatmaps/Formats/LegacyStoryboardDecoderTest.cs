// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osuTK;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Storyboards;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Beatmaps.Formats
{
    [TestFixture]
    public class LegacyStoryboardDecoderTest
    {
        [Test]
        public void TestDecodeStoryboardEvents()
        {
            var decoder = new LegacyStoryboardDecoder();

            using (var resStream = TestResources.OpenResource("Himeringo - Yotsuya-san ni Yoroshiku (RLC) [Winber1's Extreme].osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var storyboard = decoder.Decode(stream);

                Assert.IsTrue(storyboard.HasDrawable);
                Assert.AreEqual(4, storyboard.Layers.Count());

                StoryboardLayer background = storyboard.Layers.FirstOrDefault(l => l.Depth == 3);
                Assert.IsNotNull(background);
                Assert.AreEqual(16, background.Elements.Count);
                Assert.IsTrue(background.EnabledWhenFailing);
                Assert.IsTrue(background.EnabledWhenPassing);
                Assert.AreEqual("Background", background.Name);

                StoryboardLayer fail = storyboard.Layers.FirstOrDefault(l => l.Depth == 2);
                Assert.IsNotNull(fail);
                Assert.AreEqual(0, fail.Elements.Count);
                Assert.IsTrue(fail.EnabledWhenFailing);
                Assert.IsFalse(fail.EnabledWhenPassing);
                Assert.AreEqual("Fail", fail.Name);

                StoryboardLayer pass = storyboard.Layers.FirstOrDefault(l => l.Depth == 1);
                Assert.IsNotNull(pass);
                Assert.AreEqual(0, pass.Elements.Count);
                Assert.IsFalse(pass.EnabledWhenFailing);
                Assert.IsTrue(pass.EnabledWhenPassing);
                Assert.AreEqual("Pass", pass.Name);

                StoryboardLayer foreground = storyboard.Layers.FirstOrDefault(l => l.Depth == 0);
                Assert.IsNotNull(foreground);
                Assert.AreEqual(151, foreground.Elements.Count);
                Assert.IsTrue(foreground.EnabledWhenFailing);
                Assert.IsTrue(foreground.EnabledWhenPassing);
                Assert.AreEqual("Foreground", foreground.Name);

                int spriteCount = background.Elements.Count(x => x.GetType() == typeof(StoryboardSprite));
                int animationCount = background.Elements.Count(x => x.GetType() == typeof(StoryboardAnimation));
                int sampleCount = background.Elements.Count(x => x.GetType() == typeof(StoryboardSampleInfo));

                Assert.AreEqual(15, spriteCount);
                Assert.AreEqual(1, animationCount);
                Assert.AreEqual(0, sampleCount);
                Assert.AreEqual(background.Elements.Count, spriteCount + animationCount + sampleCount);

                var sprite = background.Elements.ElementAt(0) as StoryboardSprite;
                Assert.NotNull(sprite);
                Assert.IsTrue(sprite.HasCommands);
                Assert.AreEqual(new Vector2(320, 240), sprite.InitialPosition);
                Assert.IsTrue(sprite.IsDrawable);
                Assert.AreEqual(Anchor.Centre, sprite.Origin);
                Assert.AreEqual("SB/black.jpg", sprite.Path);

                var animation = background.Elements.OfType<StoryboardAnimation>().First();
                Assert.NotNull(animation);
                Assert.AreEqual(141175, animation.EndTime);
                Assert.AreEqual(10, animation.FrameCount);
                Assert.AreEqual(30, animation.FrameDelay);
                Assert.IsTrue(animation.HasCommands);
                Assert.AreEqual(new Vector2(320, 240), animation.InitialPosition);
                Assert.IsTrue(animation.IsDrawable);
                Assert.AreEqual(AnimationLoopType.LoopForever, animation.LoopType);
                Assert.AreEqual(Anchor.Centre, animation.Origin);
                Assert.AreEqual("SB/red jitter/red_0000.jpg", animation.Path);
                Assert.AreEqual(78993, animation.StartTime);
            }
        }

        [Test]
        public void TestDecodeVariableWithSuffix()
        {
            var decoder = new LegacyStoryboardDecoder();

            using (var resStream = TestResources.OpenResource("variable-with-suffix.osb"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var storyboard = decoder.Decode(stream);

                StoryboardLayer background = storyboard.Layers.Single(l => l.Depth == 3);
                Assert.AreEqual(123456, ((StoryboardSprite)background.Elements.Single()).InitialPosition.X);
            }
        }
    }
}
