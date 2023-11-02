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
                Assert.AreEqual(6, storyboard.Layers.Count());

                StoryboardLayer background = storyboard.Layers.Single(l => l.Depth == 3);
                Assert.IsNotNull(background);
                Assert.AreEqual(16, background.Elements.Count);
                Assert.IsTrue(background.VisibleWhenFailing);
                Assert.IsTrue(background.VisibleWhenPassing);
                Assert.AreEqual("Background", background.Name);

                StoryboardLayer fail = storyboard.Layers.Single(l => l.Depth == 2);
                Assert.IsNotNull(fail);
                Assert.AreEqual(0, fail.Elements.Count);
                Assert.IsTrue(fail.VisibleWhenFailing);
                Assert.IsFalse(fail.VisibleWhenPassing);
                Assert.AreEqual("Fail", fail.Name);

                StoryboardLayer pass = storyboard.Layers.Single(l => l.Depth == 1);
                Assert.IsNotNull(pass);
                Assert.AreEqual(0, pass.Elements.Count);
                Assert.IsFalse(pass.VisibleWhenFailing);
                Assert.IsTrue(pass.VisibleWhenPassing);
                Assert.AreEqual("Pass", pass.Name);

                StoryboardLayer foreground = storyboard.Layers.Single(l => l.Depth == 0);
                Assert.IsNotNull(foreground);
                Assert.AreEqual(151, foreground.Elements.Count);
                Assert.IsTrue(foreground.VisibleWhenFailing);
                Assert.IsTrue(foreground.VisibleWhenPassing);
                Assert.AreEqual("Foreground", foreground.Name);

                StoryboardLayer overlay = storyboard.Layers.Single(l => l.Depth == int.MinValue);
                Assert.IsNotNull(overlay);
                Assert.IsEmpty(overlay.Elements);
                Assert.IsTrue(overlay.VisibleWhenFailing);
                Assert.IsTrue(overlay.VisibleWhenPassing);
                Assert.AreEqual("Overlay", overlay.Name);

                int spriteCount = background.Elements.Count(x => x.GetType() == typeof(StoryboardSprite));
                int animationCount = background.Elements.Count(x => x.GetType() == typeof(StoryboardAnimation));
                int sampleCount = background.Elements.Count(x => x.GetType() == typeof(StoryboardSampleInfo));

                Assert.AreEqual(15, spriteCount);
                Assert.AreEqual(1, animationCount);
                Assert.AreEqual(0, sampleCount);
                Assert.AreEqual(background.Elements.Count, spriteCount + animationCount + sampleCount);

                var sprite = background.Elements.ElementAt(0) as StoryboardSprite;
                Assert.NotNull(sprite);
                Assert.IsTrue(sprite!.HasCommands);
                Assert.AreEqual(new Vector2(320, 240), sprite.InitialPosition);
                Assert.IsTrue(sprite.IsDrawable);
                Assert.AreEqual(Anchor.Centre, sprite.Origin);
                Assert.AreEqual("SB/lyric/ja-21.png", sprite.Path);

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
        public void TestLoopWithoutExplicitFadeOut()
        {
            var decoder = new LegacyStoryboardDecoder();

            using (var resStream = TestResources.OpenResource("animation-loop-no-explicit-end-time.osb"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var storyboard = decoder.Decode(stream);

                StoryboardLayer background = storyboard.Layers.Single(l => l.Depth == 3);
                Assert.AreEqual(1, background.Elements.Count);

                Assert.AreEqual(2000, background.Elements[0].StartTime);
                Assert.AreEqual(2000, (background.Elements[0] as StoryboardAnimation)?.EarliestTransformTime);

                Assert.AreEqual(3000, (background.Elements[0] as StoryboardAnimation)?.GetEndTime());
                Assert.AreEqual(12000, (background.Elements[0] as StoryboardAnimation)?.EndTimeForDisplay);
            }
        }

        [Test]
        public void TestCorrectAnimationStartTime()
        {
            var decoder = new LegacyStoryboardDecoder();

            using (var resStream = TestResources.OpenResource("animation-starts-before-alpha.osb"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var storyboard = decoder.Decode(stream);

                StoryboardLayer background = storyboard.Layers.Single(l => l.Depth == 3);
                Assert.AreEqual(1, background.Elements.Count);

                Assert.AreEqual(2000, background.Elements[0].StartTime);
                // This property should be used in DrawableStoryboardAnimation as a starting point for animation playback.
                Assert.AreEqual(1000, (background.Elements[0] as StoryboardAnimation)?.EarliestTransformTime);
            }
        }

        [Test]
        public void TestOutOfOrderStartTimes()
        {
            var decoder = new LegacyStoryboardDecoder();

            using (var resStream = TestResources.OpenResource("out-of-order-starttimes.osb"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var storyboard = decoder.Decode(stream);

                StoryboardLayer background = storyboard.Layers.Single(l => l.Depth == 3);
                Assert.AreEqual(2, background.Elements.Count);

                Assert.AreEqual(1500, background.Elements[0].StartTime);
                Assert.AreEqual(1000, background.Elements[1].StartTime);

                Assert.AreEqual(1000, storyboard.EarliestEventTime);
            }
        }

        [Test]
        public void TestEarliestStartTimeWithLoopAlphas()
        {
            var decoder = new LegacyStoryboardDecoder();

            using (var resStream = TestResources.OpenResource("loop-containing-earlier-non-zero-fade.osb"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var storyboard = decoder.Decode(stream);

                StoryboardLayer background = storyboard.Layers.Single(l => l.Depth == 3);
                Assert.AreEqual(2, background.Elements.Count);

                Assert.AreEqual(1000, background.Elements[0].StartTime);
                Assert.AreEqual(1000, background.Elements[1].StartTime);

                Assert.AreEqual(1000, storyboard.EarliestEventTime);
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
                Assert.AreEqual(3456, ((StoryboardSprite)background.Elements.Single()).InitialPosition.X);
            }
        }

        [Test]
        public void TestDecodeVideoWithLowercaseExtension()
        {
            var decoder = new LegacyStoryboardDecoder();

            using (var resStream = TestResources.OpenResource("video-with-lowercase-extension.osb"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var storyboard = decoder.Decode(stream);

                StoryboardLayer video = storyboard.Layers.Single(l => l.Name == "Video");
                Assert.That(video.Elements.Count, Is.EqualTo(1));

                Assert.AreEqual("Video.avi", ((StoryboardVideo)video.Elements[0]).Path);
            }
        }

        [Test]
        public void TestDecodeVideoWithUppercaseExtension()
        {
            var decoder = new LegacyStoryboardDecoder();

            using (var resStream = TestResources.OpenResource("video-with-uppercase-extension.osb"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var storyboard = decoder.Decode(stream);

                StoryboardLayer video = storyboard.Layers.Single(l => l.Name == "Video");
                Assert.That(video.Elements.Count, Is.EqualTo(1));

                Assert.AreEqual("Video.AVI", ((StoryboardVideo)video.Elements[0]).Path);
            }
        }

        [Test]
        public void TestDecodeImageSpecifiedAsVideo()
        {
            var decoder = new LegacyStoryboardDecoder();

            using (var resStream = TestResources.OpenResource("image-specified-as-video.osb"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var storyboard = decoder.Decode(stream);

                StoryboardLayer video = storyboard.Layers.Single(l => l.Name == "Video");
                Assert.That(video.Elements.Count, Is.Zero);
            }
        }

        [Test]
        public void TestDecodeOutOfRangeLoopAnimationType()
        {
            var decoder = new LegacyStoryboardDecoder();

            using (var resStream = TestResources.OpenResource("animation-types.osb"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var storyboard = decoder.Decode(stream);

                StoryboardLayer foreground = storyboard.Layers.Single(l => l.Depth == 0);
                Assert.AreEqual(AnimationLoopType.LoopForever, ((StoryboardAnimation)foreground.Elements[0]).LoopType);
                Assert.AreEqual(AnimationLoopType.LoopOnce, ((StoryboardAnimation)foreground.Elements[1]).LoopType);
                Assert.AreEqual(AnimationLoopType.LoopForever, ((StoryboardAnimation)foreground.Elements[2]).LoopType);
                Assert.AreEqual(AnimationLoopType.LoopOnce, ((StoryboardAnimation)foreground.Elements[3]).LoopType);
                Assert.AreEqual(AnimationLoopType.LoopForever, ((StoryboardAnimation)foreground.Elements[4]).LoopType);
                Assert.AreEqual(AnimationLoopType.LoopForever, ((StoryboardAnimation)foreground.Elements[5]).LoopType);
            }
        }

        [Test]
        public void TestDecodeLoopCount()
        {
            // all loop sequences in loop-count.osb have a total duration of 2000ms (fade in 0->1000ms, fade out 1000->2000ms).
            const double loop_duration = 2000;

            var decoder = new LegacyStoryboardDecoder();

            using (var resStream = TestResources.OpenResource("loop-count.osb"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var storyboard = decoder.Decode(stream);

                StoryboardLayer background = storyboard.Layers.Single(l => l.Depth == 3);

                // stable ensures that any loop command executes at least once, even if the loop count specified in the .osb is zero or negative.
                StoryboardSprite zeroTimes = background.Elements.OfType<StoryboardSprite>().Single(s => s.Path == "zero-times.png");
                Assert.That(zeroTimes.EndTime, Is.EqualTo(1000 + loop_duration));

                StoryboardSprite oneTime = background.Elements.OfType<StoryboardSprite>().Single(s => s.Path == "one-time.png");
                Assert.That(oneTime.EndTime, Is.EqualTo(4000 + loop_duration));

                StoryboardSprite manyTimes = background.Elements.OfType<StoryboardSprite>().Single(s => s.Path == "many-times.png");
                // It is intentional that we don't consider the loop count (40) as part of the end time calculation to match stable's handling.
                // If we were to include the loop count, storyboards which loop for stupid long loop counts would continue playing the outro forever.
                Assert.That(manyTimes.EndTime, Is.EqualTo(9000 + loop_duration));
            }
        }

        [Test]
        public void TestVideoAndBackgroundEventsDoNotAffectStoryboardBounds()
        {
            var decoder = new LegacyStoryboardDecoder();

            using var resStream = TestResources.OpenResource("video-background-events-ignored.osb");
            using var stream = new LineBufferedReader(resStream);

            var storyboard = decoder.Decode(stream);

            Assert.Multiple(() =>
            {
                Assert.That(storyboard.GetLayer(@"Video").Elements, Has.Count.EqualTo(1));
                Assert.That(storyboard.GetLayer(@"Video").Elements.Single(), Is.InstanceOf<StoryboardVideo>());
                Assert.That(storyboard.GetLayer(@"Video").Elements.Single().StartTime, Is.EqualTo(-5678));

                Assert.That(storyboard.EarliestEventTime, Is.Null);
                Assert.That(storyboard.LatestEventTime, Is.Null);
            });
        }
    }
}
