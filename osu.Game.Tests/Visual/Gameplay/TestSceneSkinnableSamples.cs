// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneSkinnableSamples : OsuTestScene
    {
        private TestSkinSourceContainer skinSource = null!;
        private SkinnableSamples skinnableSamples = null!;

        private const string sample_lookup = "Gameplay/normal-sliderslide";

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("setup hierarchy", () =>
            {
                Child = skinSource = new TestSkinSourceContainer
                {
                    RelativeSizeAxes = Axes.Both,
                };

                // has to be added after the hierarchy above else the `ISkinSource` dependency won't be cached.
                skinSource.Add(skinnableSamples = new SkinnableSamples(new SampleInfo(sample_lookup)));
            });
        }

        [Test]
        public void TestStoppedSoundDoesntResumeAfterPause()
        {
            AddStep("start sample with looping", () =>
            {
                skinnableSamples.Looping = true;
                skinnableSamples.Play();
            });

            AddUntilStep("wait for sample to start playing", () => skinnableSamples.IsActivelyPlaying);

            AddStep("stop sample", () => skinnableSamples.Stop());

            AddUntilStep("wait for sample to stop playing", () => !skinnableSamples.IsActivelyPlaying);

            AddStep("disable sample playback", () => skinSource.SamplePlaybackDisabled.Value = true);

            AddStep("enable sample playback", () => skinSource.SamplePlaybackDisabled.Value = false);

            AddWaitStep("wait a bit", 5);
            AddAssert("sample not playing", () => !skinnableSamples.IsActivelyPlaying);
        }

        [Test]
        public void TestLoopingSoundResumesAfterPause()
        {
            AddStep("start sample with looping", () =>
            {
                skinnableSamples.Looping = true;
                skinnableSamples.Play();
            });

            AddUntilStep("wait for sample to start playing", () => skinnableSamples.IsActivelyPlaying);

            AddStep("disable sample playback", () => skinSource.SamplePlaybackDisabled.Value = true);
            AddUntilStep("wait for sample to stop playing", () => !skinnableSamples.IsActivelyPlaying);

            AddStep("enable sample playback", () => skinSource.SamplePlaybackDisabled.Value = false);
            AddUntilStep("wait for sample to start playing", () => skinnableSamples.IsActivelyPlaying);
        }

        [Test]
        public void TestNonLoopingStopsWithPause()
        {
            AddStep("start sample", () => skinnableSamples.Play());

            AddAssert("sample playing", () => skinnableSamples.IsActivelyPlaying);

            AddStep("disable sample playback", () => skinSource.SamplePlaybackDisabled.Value = true);

            AddUntilStep("sample not playing", () => !skinnableSamples.IsActivelyPlaying);

            AddStep("enable sample playback", () => skinSource.SamplePlaybackDisabled.Value = false);

            AddAssert("sample not playing", () => !skinnableSamples.IsActivelyPlaying);
            AddAssert("sample not playing", () => !skinnableSamples.IsActivelyPlaying);
            AddAssert("sample not playing", () => !skinnableSamples.IsActivelyPlaying);
        }

        [Test]
        public void TestSampleUpdatedBeforePlaybackWhenNotPresent()
        {
            AddStep("make sample non-present", () => skinnableSamples.Hide());
            AddUntilStep("ensure not present", () => skinnableSamples.IsPresent, () => Is.False);

            AddUntilStep("ensure sample loaded", () => skinnableSamples.ChildrenOfType<DrawableSample>().Single().Name, () => Is.EqualTo(sample_lookup));

            AddStep("change source", () =>
            {
                skinSource.OverridingSample = new SampleVirtual("new skin");
                skinSource.TriggerSourceChanged();
            });

            AddStep("start sample", () => skinnableSamples.Play());
            AddUntilStep("sample updated", () => skinnableSamples.ChildrenOfType<DrawableSample>().Single().Name, () => Is.EqualTo("new skin"));
        }

        [Test]
        public void TestSkinChangeDoesntPlayOnPause()
        {
            DrawableSample? sample = null;
            AddStep("start sample", () =>
            {
                skinnableSamples.Play();
                sample = skinnableSamples.ChildrenOfType<DrawableSample>().Single();
            });

            AddAssert("sample playing", () => skinnableSamples.IsActivelyPlaying);

            AddStep("disable sample playback", () => skinSource.SamplePlaybackDisabled.Value = true);
            AddUntilStep("wait for sample to stop playing", () => !skinnableSamples.IsActivelyPlaying);

            AddStep("trigger skin change", () => skinSource.TriggerSourceChanged());

            AddAssert("retrieve and ensure current sample is different", () =>
            {
                DrawableSample? oldSample = sample;
                sample = skinnableSamples.ChildrenOfType<DrawableSample>().Single();
                return sample != oldSample;
            });

            AddAssert("new sample stopped", () => !skinnableSamples.IsActivelyPlaying);
            AddStep("enable sample playback", () => skinSource.SamplePlaybackDisabled.Value = false);

            AddWaitStep("wait a bit", 5);
            AddAssert("new sample not played", () => !skinnableSamples.IsActivelyPlaying);
        }

        [Cached(typeof(ISkinSource))]
        private partial class TestSkinSourceContainer : Container, ISkinSource, ISamplePlaybackDisabler
        {
            [Resolved]
            private ISkinSource source { get; set; } = null!;

            public event Action? SourceChanged;

            public Bindable<bool> SamplePlaybackDisabled { get; } = new Bindable<bool>();

            public ISample? OverridingSample;

            IBindable<bool> ISamplePlaybackDisabler.SamplePlaybackDisabled => SamplePlaybackDisabled;

            public Drawable? GetDrawableComponent(ISkinComponentLookup lookup) => source.GetDrawableComponent(lookup);
            public Texture? GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => source.GetTexture(componentName, wrapModeS, wrapModeT);
            public ISample? GetSample(ISampleInfo sampleInfo) => OverridingSample ?? source.GetSample(sampleInfo);

            public IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup)
                where TLookup : notnull
                where TValue : notnull
            {
                return source.GetConfig<TLookup, TValue>(lookup);
            }

            public ISkin? FindProvider(Func<ISkin, bool> lookupFunction) => lookupFunction(this) ? this : source.FindProvider(lookupFunction);
            public IEnumerable<ISkin> AllSources => new[] { this }.Concat(source.AllSources);

            public void TriggerSourceChanged()
            {
                SourceChanged?.Invoke();
            }
        }
    }
}
