// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Skinning;
using osu.Game.Audio;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual;
using osu.Framework.IO.Stores;
using osu.Framework.Testing;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;

namespace osu.Game.Tests.Skins
{
    [HeadlessTest]
    public class TestSceneLegacySkin : OsuTestScene
    {
        [Test]
        public void TestLayeredHitSoundsHandledCorrectly()
        {
            var hsiLayered = new TestHitSampleInfo(true);
            var hsiUnlayered = new TestHitSampleInfo(false);

            LegacySkin skinLayered = null;
            LegacySkin skinUnlayered = null;
            SampleChannel channel = null;

            AddStep("create skin with LayeredHitSounds on", () => skinLayered = new TestSkin(Audio, true));
            AddStep("create skin with LayeredHitSounds off", () => skinUnlayered = new TestSkin(Audio, false));

            AddStep("retrieve sample", () => channel = skinLayered.GetSample(hsiLayered));
            AddAssert("sample is non-null", () => channel != null, "LayeredHitSounds ON and IsLayered ON");

            AddStep("retrieve sample", () => channel = skinLayered.GetSample(hsiUnlayered));
            AddAssert("sample is non-null", () => channel != null, "LayeredHitSounds ON and IsLayered OFF");

            AddStep("retrieve sample", () => channel = skinUnlayered.GetSample(hsiLayered));
            AddAssert("sample is null", () => channel == null, "LayeredHitSounds OFF and IsLayered ON");

            AddStep("retrieve sample", () => channel = skinUnlayered.GetSample(hsiUnlayered));
            AddAssert("sample is non-null", () => channel != null, "LayeredHitSounds OFF and IsLayered OFF");
        }

        [Test]
        public void TestFallbackForNonCustomSample()
        {
            LegacySkin skin = null;
            SampleChannel channel = null;

            AddStep("create skin", () => skin = new TestSkin(Audio));

            AddStep("retrieve custom sample", () => channel = skin.GetSample(new TestHitSampleInfo(false, true)));
            AddAssert("sample is non-null", () => channel != null);

            AddStep("retrieve non-custom sample", () => channel = skin.GetSample(new TestHitSampleInfo(false, false)));
            AddAssert("sample is null", () => channel == null);
        }

        private class TestSkin : LegacySkin
        {
            public TestSkin(AudioManager audioManager, bool layeredHitSounds = true, string resourceName = "normal-hitnormal")
                : base(new SkinInfo(), new TestResourceStore(resourceName), audioManager, string.Empty)
            {
                Configuration.ConfigDictionary["LayeredHitSounds"] = layeredHitSounds ? "1" : "0";
            }
        }

        private class TestHitSampleInfo : HitSampleInfo
        {
            public TestHitSampleInfo(bool isLayered, bool isCustom = true)
            {
                Bank = "normal";
                Name = "hitnormal";
                IsLayered = isLayered;
                IsCustom = isCustom;
            }
        }

        private class TestResourceStore : IResourceStore<byte[]>
        {
            private readonly string resourceName;

            public TestResourceStore(string resourceName)
            {
                this.resourceName = resourceName;
            }

            public byte[] Get(string name) => name == resourceName ? TestResources.GetStore().Get("Resources/test-sample.mp3") : null;

            public Task<byte[]> GetAsync(string name) => name == resourceName ? TestResources.GetStore().GetAsync("Resources/test-sample.mp3") : null;

            public Stream GetStream(string name) => name == resourceName ? TestResources.GetStore().GetStream("Resources/test-sample.mp3") : null;

            public IEnumerable<string> GetAvailableResources() => new[] { resourceName };

            public void Dispose()
            {
            }
        }
    }
}
