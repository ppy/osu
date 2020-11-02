// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Screens.Play;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSkinnableSound : OsuTestScene
    {
        private TestSkinSourceContainer skinSource;
        private PausableSkinnableSound skinnableSound;

        [SetUp]
        public void SetUpSteps()
        {
            AddStep("setup hierarchy", () =>
            {
                Children = new Drawable[]
                {
                    skinSource = new TestSkinSourceContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = skinnableSound = new PausableSkinnableSound(new SampleInfo("Gameplay/normal-sliderslide"))
                    },
                };
            });
        }

        [Test]
        public void TestStoppedSoundDoesntResumeAfterPause()
        {
            DrawableSample sample = null;
            AddStep("start sample with looping", () =>
            {
                sample = skinnableSound.ChildrenOfType<DrawableSample>().First();

                skinnableSound.Looping = true;
                skinnableSound.Play();
            });

            AddUntilStep("wait for sample to start playing", () => sample.Playing);

            AddStep("stop sample", () => skinnableSound.Stop());

            AddUntilStep("wait for sample to stop playing", () => !sample.Playing);

            AddStep("disable sample playback", () => skinSource.SamplePlaybackDisabled.Value = true);

            AddStep("enable sample playback", () => skinSource.SamplePlaybackDisabled.Value = false);

            AddWaitStep("wait a bit", 5);
            AddAssert("sample not playing", () => !sample.Playing);
        }

        [Test]
        public void TestLoopingSoundResumesAfterPause()
        {
            DrawableSample sample = null;
            AddStep("start sample with looping", () =>
            {
                skinnableSound.Looping = true;
                skinnableSound.Play();
                sample = skinnableSound.ChildrenOfType<DrawableSample>().First();
            });

            AddUntilStep("wait for sample to start playing", () => sample.Playing);

            AddStep("disable sample playback", () => skinSource.SamplePlaybackDisabled.Value = true);
            AddUntilStep("wait for sample to stop playing", () => !sample.Playing);

            AddStep("enable sample playback", () => skinSource.SamplePlaybackDisabled.Value = false);
            AddUntilStep("wait for sample to start playing", () => sample.Playing);
        }

        [Test]
        public void TestNonLoopingStopsWithPause()
        {
            DrawableSample sample = null;
            AddStep("start sample", () =>
            {
                skinnableSound.Play();
                sample = skinnableSound.ChildrenOfType<DrawableSample>().First();
            });

            AddAssert("sample playing", () => sample.Playing);

            AddStep("disable sample playback", () => skinSource.SamplePlaybackDisabled.Value = true);

            AddUntilStep("sample not playing", () => !sample.Playing);

            AddStep("enable sample playback", () => skinSource.SamplePlaybackDisabled.Value = false);

            AddAssert("sample not playing", () => !sample.Playing);
            AddAssert("sample not playing", () => !sample.Playing);
            AddAssert("sample not playing", () => !sample.Playing);
        }

        [Test]
        public void TestSkinChangeDoesntPlayOnPause()
        {
            DrawableSample sample = null;
            AddStep("start sample", () =>
            {
                skinnableSound.Play();
                sample = skinnableSound.ChildrenOfType<DrawableSample>().Single();
            });

            AddAssert("sample playing", () => sample.Playing);

            AddStep("disable sample playback", () => skinSource.SamplePlaybackDisabled.Value = true);
            AddUntilStep("wait for sample to stop playing", () => !sample.Playing);

            AddStep("trigger skin change", () => skinSource.TriggerSourceChanged());

            AddAssert("retrieve and ensure current sample is different", () =>
            {
                DrawableSample oldSample = sample;
                sample = skinnableSound.ChildrenOfType<DrawableSample>().Single();
                return sample != oldSample;
            });

            AddAssert("new sample stopped", () => !sample.Playing);
            AddStep("enable sample playback", () => skinSource.SamplePlaybackDisabled.Value = false);

            AddWaitStep("wait a bit", 5);
            AddAssert("new sample not played", () => !sample.Playing);
        }

        [Cached(typeof(ISkinSource))]
        [Cached(typeof(ISamplePlaybackDisabler))]
        private class TestSkinSourceContainer : Container, ISkinSource, ISamplePlaybackDisabler
        {
            [Resolved]
            private ISkinSource source { get; set; }

            public event Action SourceChanged;

            public Bindable<bool> SamplePlaybackDisabled { get; } = new Bindable<bool>();

            IBindable<bool> ISamplePlaybackDisabler.SamplePlaybackDisabled => SamplePlaybackDisabled;

            public Drawable GetDrawableComponent(ISkinComponent component) => source?.GetDrawableComponent(component);
            public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => source?.GetTexture(componentName, wrapModeS, wrapModeT);
            public SampleChannel GetSample(ISampleInfo sampleInfo) => source?.GetSample(sampleInfo);
            public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => source?.GetConfig<TLookup, TValue>(lookup);

            public void TriggerSourceChanged()
            {
                SourceChanged?.Invoke();
            }
        }
    }
}
