// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.IO.Stores;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Skinning;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Gameplay
{
    [HeadlessTest]
    public class TestSceneStoryboardSamples : OsuTestScene
    {
        [Test]
        public void TestRetrieveTopLevelSample()
        {
            ISkin skin = null;
            SampleChannel channel = null;

            AddStep("create skin", () => skin = new TestSkin("test-sample", Audio));
            AddStep("retrieve sample", () => channel = skin.GetSample(new SampleInfo("test-sample")));

            AddAssert("sample is non-null", () => channel != null);
        }

        [Test]
        public void TestRetrieveSampleInSubFolder()
        {
            ISkin skin = null;
            SampleChannel channel = null;

            AddStep("create skin", () => skin = new TestSkin("folder/test-sample", Audio));
            AddStep("retrieve sample", () => channel = skin.GetSample(new SampleInfo("folder/test-sample")));

            AddAssert("sample is non-null", () => channel != null);
        }

        private class TestSkin : LegacySkin
        {
            public TestSkin(string resourceName, AudioManager audioManager)
                : base(DefaultLegacySkin.Info, new TestResourceStore(resourceName), audioManager, "skin.ini")
            {
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
