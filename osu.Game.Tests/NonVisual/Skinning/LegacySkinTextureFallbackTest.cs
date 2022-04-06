// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Audio;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.Skinning;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace osu.Game.Tests.NonVisual.Skinning
{
    [TestFixture]
    public sealed class LegacySkinTextureFallbackTest
    {
        private static object[][] fallbackTestCases =
        {
            new object[]
            {
                // textures in store
                new[] { "Gameplay/osu/followpoint@2x", "Gameplay/osu/followpoint" },
                // requested component
                "Gameplay/osu/followpoint",
                // returned texture name & scale
                "Gameplay/osu/followpoint@2x", 2
            },
            new object[]
            {
                new[] { "Gameplay/osu/followpoint@2x" },
                "Gameplay/osu/followpoint",
                "Gameplay/osu/followpoint@2x", 2
            },
            new object[]
            {
                new[] { "Gameplay/osu/followpoint" },
                "Gameplay/osu/followpoint",
                "Gameplay/osu/followpoint", 1
            },
            new object[]
            {
                new[] { "Gameplay/osu/followpoint", "followpoint@2x" },
                "Gameplay/osu/followpoint",
                "Gameplay/osu/followpoint", 1
            },
            new object[]
            {
                new[] { "followpoint@2x", "followpoint" },
                "Gameplay/osu/followpoint",
                "followpoint@2x", 2
            },
            new object[]
            {
                new[] { "followpoint@2x" },
                "Gameplay/osu/followpoint",
                "followpoint@2x", 2
            },
            new object[]
            {
                new[] { "followpoint" },
                "Gameplay/osu/followpoint",
                "followpoint", 1
            },
        };

        [TestCaseSource(nameof(fallbackTestCases))]
        public void TestFallbackOrder(string[] filesInStore, string requestedComponent, string expectedTexture, float expectedScale)
        {
            var textureStore = new TestTextureStore(filesInStore);
            var legacySkin = new TestLegacySkin(textureStore);

            var texture = legacySkin.GetTexture(requestedComponent);

            Assert.IsNotNull(texture);
            Assert.AreEqual(textureStore.Textures[expectedTexture].Width, texture.Width);
            Assert.AreEqual(expectedScale, texture.ScaleAdjust);
        }

        [Test]
        public void TestReturnNullOnFallbackFailure()
        {
            var textureStore = new TestTextureStore("sliderb", "hit100");
            var legacySkin = new TestLegacySkin(textureStore);

            var texture = legacySkin.GetTexture("Gameplay/osu/followpoint");

            Assert.IsNull(texture);
        }

        private class TestLegacySkin : LegacySkin
        {
            public TestLegacySkin(IResourceStore<TextureUpload> textureStore)
                : base(new SkinInfo(), new TestResourceProvider(textureStore), null, string.Empty)
            {
            }

            private class TestResourceProvider : IStorageResourceProvider
            {
                private readonly IResourceStore<TextureUpload> textureStore;

                public TestResourceProvider(IResourceStore<TextureUpload> textureStore)
                {
                    this.textureStore = textureStore;
                }

                public AudioManager AudioManager => null;
                public IResourceStore<byte[]> Files => null;
                public IResourceStore<byte[]> Resources => null;
                public RealmAccess RealmAccess => null;
                public IResourceStore<TextureUpload> CreateTextureLoaderStore(IResourceStore<byte[]> underlyingStore) => textureStore;
            }
        }

        private class TestTextureStore : IResourceStore<TextureUpload>
        {
            public readonly Dictionary<string, TextureUpload> Textures;

            public TestTextureStore(params string[] fileNames)
            {
                // use an incrementing width to allow assertion matching on correct textures as they turn from uploads into actual textures.
                int width = 1;
                Textures = fileNames.ToDictionary(fileName => fileName, fileName => new TextureUpload(new Image<Rgba32>(width, width++)));
            }

            public TextureUpload Get(string name) => Textures.GetValueOrDefault(name);

            public Task<TextureUpload> GetAsync(string name, CancellationToken cancellationToken = new CancellationToken()) => Task.FromResult(Get(name));

            public Stream GetStream(string name) => throw new NotImplementedException();

            public IEnumerable<string> GetAvailableResources() => throw new NotImplementedException();

            public void Dispose()
            {
            }
        }
    }
}
