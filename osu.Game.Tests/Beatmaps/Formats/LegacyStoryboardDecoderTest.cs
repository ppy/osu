// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
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

                ClassicAssert.True(storyboard.HasDrawable);
                ClassicAssert.AreEqual(6, storyboard.Layers.Count());

                StoryboardLayer background = storyboard.Layers.Single(l => l.Depth == 3);
                ClassicAssert.NotNull(background);
                ClassicAssert.AreEqual(16, background.Elements.Count);
                ClassicAssert.True(background.VisibleWhenFailing);
                ClassicAssert.True(background.VisibleWhenPassing);
                ClassicAssert.AreEqual("Background", background.Name);

                StoryboardLayer fail = storyboard.Layers.Single(l => l.Depth == 2);
                ClassicAssert.NotNull(fail);
                ClassicAssert.AreEqual(0, fail.Elements.Count);
                ClassicAssert.True(fail.VisibleWhenFailing);
                ClassicAssert.False(fail.VisibleWhenPassing);
                ClassicAssert.AreEqual("Fail", fail.Name);

                StoryboardLayer pass = storyboard.Layers.Single(l => l.Depth == 1);
                ClassicAssert.NotNull(pass);
                ClassicAssert.AreEqual(0, pass.Elements.Count);
                ClassicAssert.False(pass.VisibleWhenFailing);
                ClassicAssert.True(pass.VisibleWhenPassing);
                ClassicAssert.AreEqual("Pass", pass.Name);

                StoryboardLayer foreground = storyboard.Layers.Single(l => l.Depth == 0);
                ClassicAssert.NotNull(foreground);
                ClassicAssert.AreEqual(151, foreground.Elements.Count);
                ClassicAssert.True(foreground.VisibleWhenFailing);
                ClassicAssert.True(foreground.VisibleWhenPassing);
                ClassicAssert.AreEqual("Foreground", foreground.Name);

                StoryboardLayer overlay = storyboard.Layers.Single(l => l.Depth == int.MinValue);
                ClassicAssert.NotNull(overlay);
                ClassicAssert.IsEmpty(overlay.Elements);
                ClassicAssert.True(overlay.VisibleWhenFailing);
                ClassicAssert.True(overlay.VisibleWhenPassing);
                ClassicAssert.AreEqual("Overlay", overlay.Name);

                int spriteCount = background.Elements.Count(x => x.GetType() == typeof(StoryboardSprite));
                int animationCount = background.Elements.Count(x => x.GetType() == typeof(StoryboardAnimation));
                int sampleCount = background.Elements.Count(x => x.GetType() == typeof(StoryboardSampleInfo));

                ClassicAssert.AreEqual(15, spriteCount);
                ClassicAssert.AreEqual(1, animationCount);
                ClassicAssert.AreEqual(0, sampleCount);
                ClassicAssert.AreEqual(background.Elements.Count, spriteCount + animationCount + sampleCount);

                var sprite = background.Elements.ElementAt(0) as StoryboardSprite;
                ClassicAssert.NotNull(sprite);
                ClassicAssert.True(sprite!.HasCommands);
                ClassicAssert.AreEqual(new Vector2(320, 240), sprite.InitialPosition);
                ClassicAssert.True(sprite.IsDrawable);
                ClassicAssert.AreEqual(Anchor.Centre, sprite.Origin);
                ClassicAssert.AreEqual("SB/lyric/ja-21.png", sprite.Path);

                var animation = background.Elements.OfType<StoryboardAnimation>().First();
                ClassicAssert.NotNull(animation);
                ClassicAssert.AreEqual(141175, animation.EndTime);
                ClassicAssert.AreEqual(10, animation.FrameCount);
                ClassicAssert.AreEqual(30, animation.FrameDelay);
                ClassicAssert.True(animation.HasCommands);
                ClassicAssert.AreEqual(new Vector2(320, 240), animation.InitialPosition);
                ClassicAssert.True(animation.IsDrawable);
                ClassicAssert.AreEqual(AnimationLoopType.LoopForever, animation.LoopType);
                ClassicAssert.AreEqual(Anchor.Centre, animation.Origin);
                ClassicAssert.AreEqual("SB/red jitter/red_0000.jpg", animation.Path);
                ClassicAssert.AreEqual(78993, animation.StartTime);
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
                ClassicAssert.AreEqual(1, background.Elements.Count);

                ClassicAssert.AreEqual(2000, background.Elements[0].StartTime);
                ClassicAssert.AreEqual(2000, (background.Elements[0] as StoryboardAnimation)?.EarliestTransformTime);

                ClassicAssert.AreEqual(3000, (background.Elements[0] as StoryboardAnimation)?.GetEndTime());
                ClassicAssert.AreEqual(12000, (background.Elements[0] as StoryboardAnimation)?.EndTimeForDisplay);
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
                ClassicAssert.AreEqual(1, background.Elements.Count);

                ClassicAssert.AreEqual(2000, background.Elements[0].StartTime);
                // This property should be used in DrawableStoryboardAnimation as a starting point for animation playback.
                ClassicAssert.AreEqual(1000, (background.Elements[0] as StoryboardAnimation)?.EarliestTransformTime);
            }
        }

        [Test]
        public void TestNoopFadeTransformIsIgnoredForLifetime()
        {
            var decoder = new LegacyStoryboardDecoder();

            using (var resStream = TestResources.OpenResource("noop-fade-transform-is-ignored-for-lifetime.osb"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var storyboard = decoder.Decode(stream);

                StoryboardLayer background = storyboard.Layers.Single(l => l.Depth == 3);
                ClassicAssert.AreEqual(2, background.Elements.Count);

                ClassicAssert.AreEqual(1500, background.Elements[0].StartTime);
                ClassicAssert.AreEqual(1500, background.Elements[1].StartTime);
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
                ClassicAssert.AreEqual(2, background.Elements.Count);

                ClassicAssert.AreEqual(1500, background.Elements[0].StartTime);
                ClassicAssert.AreEqual(1000, background.Elements[1].StartTime);

                ClassicAssert.AreEqual(1000, storyboard.EarliestEventTime);
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
                ClassicAssert.AreEqual(2, background.Elements.Count);

                ClassicAssert.AreEqual(1000, background.Elements[0].StartTime);
                ClassicAssert.AreEqual(1000, background.Elements[1].StartTime);

                ClassicAssert.AreEqual(1000, storyboard.EarliestEventTime);
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
                ClassicAssert.AreEqual(3456, ((StoryboardSprite)background.Elements.Single()).InitialPosition.X);
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

                ClassicAssert.AreEqual("Video.avi", ((StoryboardVideo)video.Elements[0]).Path);
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

                ClassicAssert.AreEqual("Video.AVI", ((StoryboardVideo)video.Elements[0]).Path);
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
                ClassicAssert.AreEqual(AnimationLoopType.LoopForever, ((StoryboardAnimation)foreground.Elements[0]).LoopType);
                ClassicAssert.AreEqual(AnimationLoopType.LoopOnce, ((StoryboardAnimation)foreground.Elements[1]).LoopType);
                ClassicAssert.AreEqual(AnimationLoopType.LoopForever, ((StoryboardAnimation)foreground.Elements[2]).LoopType);
                ClassicAssert.AreEqual(AnimationLoopType.LoopOnce, ((StoryboardAnimation)foreground.Elements[3]).LoopType);
                ClassicAssert.AreEqual(AnimationLoopType.LoopForever, ((StoryboardAnimation)foreground.Elements[4]).LoopType);
                ClassicAssert.AreEqual(AnimationLoopType.LoopForever, ((StoryboardAnimation)foreground.Elements[5]).LoopType);
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
        public void TestVideoWithCustomFadeIn()
        {
            var decoder = new LegacyStoryboardDecoder();

            using var resStream = TestResources.OpenResource("video-custom-alpha-transform.osb");
            using var stream = new LineBufferedReader(resStream);

            var storyboard = decoder.Decode(stream);

            Assert.Multiple(() =>
            {
                Assert.That(storyboard.GetLayer(@"Video").Elements, Has.Count.EqualTo(1));
                Assert.That(storyboard.GetLayer(@"Video").Elements.Single(), Is.InstanceOf<StoryboardVideo>());
                Assert.That(storyboard.GetLayer(@"Video").Elements.Single().StartTime, Is.EqualTo(-5678));
                Assert.That(((StoryboardVideo)storyboard.GetLayer(@"Video").Elements.Single()).Commands.Alpha.Single().StartTime, Is.EqualTo(1500));
                Assert.That(((StoryboardVideo)storyboard.GetLayer(@"Video").Elements.Single()).Commands.Alpha.Single().EndTime, Is.EqualTo(1600));

                Assert.That(storyboard.EarliestEventTime, Is.Null);
                Assert.That(storyboard.LatestEventTime, Is.Null);
            });
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
