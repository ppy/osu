// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics.Rendering.Dummy;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.Skinning;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Skins
{
    [HeadlessTest]
    public partial class TestSceneSkinResources : OsuTestScene
    {
        [Resolved]
        private SkinManager skins { get; set; } = null!;

        [Test]
        public void TestRetrieveOggSample()
        {
            ISkin skin = null!;

            AddStep("import skin", () => skin = importSkinFromArchives(@"ogg-skin.osk"));
            AddAssert("sample is non-null", () => skin.GetSample(new SampleInfo(@"sample")) != null);
        }

        [Test]
        public void TestRetrievalWithConflictingFilenames()
        {
            ISkin skin = null!;

            AddStep("import skin", () => skin = importSkinFromArchives(@"conflicting-filenames-skin.osk"));
            AddAssert("texture is non-null", () => skin.GetTexture(@"spinner-osu") != null);
            AddAssert("sample is non-null", () => skin.GetSample(new SampleInfo(@"spinner-osu")) != null);
        }

        [Test]
        public void TestSampleRetrievalOrder()
        {
            Mock<IStorageResourceProvider> mockResourceProvider = null!;
            Mock<IResourceStore<byte[]>> mockResourceStore = null!;
            List<string> lookedUpFileNames = null!;

            AddStep("setup mock providers provider", () =>
            {
                lookedUpFileNames = new List<string>();
                mockResourceProvider = new Mock<IStorageResourceProvider>();
                mockResourceProvider.Setup(m => m.AudioManager).Returns(Audio);
                mockResourceProvider.Setup(m => m.Renderer).Returns(new DummyRenderer());
                mockResourceStore = new Mock<IResourceStore<byte[]>>();
                mockResourceStore.Setup(r => r.Get(It.IsAny<string>()))
                                 .Callback<string>(n => lookedUpFileNames.Add(n))
                                 .Returns<byte>(null);
            });

            AddStep("query sample", () =>
            {
                TestSkin testSkin = new TestSkin(new SkinInfo(), mockResourceProvider.Object, new ResourceStore<byte[]>(mockResourceStore.Object));
                testSkin.GetSample(new SampleInfo());
            });

            AddAssert("sample lookups were in correct order", () =>
            {
                string[] lookups = lookedUpFileNames.Where(f => f.StartsWith(TestSkin.SAMPLE_NAME, StringComparison.Ordinal)).ToArray();
                return Path.GetExtension(lookups[0]) == string.Empty
                       && Path.GetExtension(lookups[1]) == ".wav"
                       && Path.GetExtension(lookups[2]) == ".mp3"
                       && Path.GetExtension(lookups[3]) == ".ogg";
            });
        }

        private Skin importSkinFromArchives(string filename)
        {
            var imported = skins.Import(new ImportTask(TestResources.OpenResource($@"Archives/{filename}"), filename)).GetResultSafely();
            return imported.PerformRead(skinInfo => skins.GetSkin(skinInfo));
        }

        private class TestSkin : Skin
        {
            public const string SAMPLE_NAME = "test-sample";

            public TestSkin(SkinInfo skin, IStorageResourceProvider? resources, IResourceStore<byte[]>? fallbackStore = null, string configurationFilename = "skin.ini")
                : base(skin, resources, fallbackStore, configurationFilename)
            {
            }

            public override Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => throw new NotImplementedException();

            public override IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => throw new NotImplementedException();

            public override ISample GetSample(ISampleInfo sampleInfo) => Samples.AsNonNull().Get(SAMPLE_NAME);
        }
    }
}
