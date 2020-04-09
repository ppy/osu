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
            LegacySkin skinLayered = null;
            LegacySkin skinUnlayered = null;
            HitSampleInfo hsiLayered = null;
            HitSampleInfo hsiUnlayered = null;
            SampleChannel channel = null;

            AddStep("create skin with LayeredHitSounds on", () => skinLayered = new TestSkin(true, Audio));
            AddStep("create skin with LayeredHitSounds off", () => skinUnlayered = new TestSkin(false, Audio));
            AddStep("create sample info with IsLayered on", () => hsiLayered = new TestHitSampleInfo(true));
            AddStep("create sample info with IsLayered off", () => hsiUnlayered = new TestHitSampleInfo(false));

            AddStep("retrieve sample", () => channel = skinLayered.GetSample(hsiLayered));
            AddAssert("sample is non-null", () => channel != null, "LayeredHitSounds ON and IsLayered ON");

            AddStep("retrieve sample", () => channel = skinLayered.GetSample(hsiUnlayered));
            AddAssert("sample is non-null", () => channel != null, "LayeredHitSounds ON and IsLayered OFF");

            AddStep("retrieve sample", () => channel = skinUnlayered.GetSample(hsiLayered));
            AddAssert("sample is null", () => channel == null, "LayeredHitSounds OFF and IsLayered ON");

            AddStep("retrieve sample", () => channel = skinUnlayered.GetSample(hsiUnlayered));
            AddAssert("sample is non-null", () => channel != null, "LayeredHitSounds OFF and IsLayered FF");
        }

        private class TestSkin : LegacySkin
        {
            public TestSkin(bool layeredHitSounds, AudioManager audioManager)
                : base(new SkinInfo(), new TestResourceStore("test-sample"), audioManager, string.Empty)
            {
                Configuration.ConfigDictionary["LayeredHitSounds"] = layeredHitSounds ? "1" : "0";
            }
        }

        private class TestHitSampleInfo : HitSampleInfo{
            public TestHitSampleInfo(bool isLayered)
            {
                Name = "test-sample";
                IsLayered = isLayered;
                IsCustom = false;
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
