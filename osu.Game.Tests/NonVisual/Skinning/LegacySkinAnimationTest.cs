// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Audio;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.NonVisual.Skinning
{
    [HeadlessTest]
    public class LegacySkinAnimationTest : OsuTestScene
    {
        private const string animation_name = "animation";
        private const int frame_count = 6;

        [Cached(typeof(IAnimationTimeReference))]
        private TestAnimationTimeReference animationTimeReference = new TestAnimationTimeReference();

        private TextureAnimation animation;

        [Test]
        public void TestAnimationTimeReferenceChange()
        {
            ISkin skin = new TestSkin();

            AddStep("get animation", () => Add(animation = (TextureAnimation)skin.GetAnimation(animation_name, true, false)));
            AddAssert("frame count correct", () => animation.FrameCount == frame_count);
            assertPlaybackPosition(0);

            AddStep("set start time to 1000", () => animationTimeReference.AnimationStartTime.Value = 1000);
            assertPlaybackPosition(0);

            AddStep("set current time to 500", () => animationTimeReference.ManualClock.CurrentTime = 500);
            assertPlaybackPosition(0);
        }

        private void assertPlaybackPosition(double expectedPosition)
            => AddAssert($"playback position is {expectedPosition}", () => animation.PlaybackPosition == expectedPosition);

        private class TestSkin : ISkin
        {
            private static readonly string[] lookup_names = Enumerable.Range(0, frame_count).Select(frame => $"{animation_name}-{frame}").ToArray();

            public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
            {
                return lookup_names.Contains(componentName) ? Texture.WhitePixel : null;
            }

            public Drawable GetDrawableComponent(ISkinComponent component) => throw new NotSupportedException();
            public ISample GetSample(ISampleInfo sampleInfo) => throw new NotSupportedException();
            public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => throw new NotSupportedException();
            public ISkin FindProvider(Func<ISkin, bool> lookupFunction) => null;
        }

        private class TestAnimationTimeReference : IAnimationTimeReference
        {
            public ManualClock ManualClock { get; }
            public IFrameBasedClock Clock { get; }
            public Bindable<double> AnimationStartTime { get; } = new BindableDouble();

            public TestAnimationTimeReference()
            {
                ManualClock = new ManualClock();
                Clock = new FramedClock(ManualClock);
            }
        }
    }
}
