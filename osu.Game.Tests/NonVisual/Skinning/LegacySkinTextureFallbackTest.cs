// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Audio;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Rendering.Dummy;
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
            new object[]
            {
                // Looking up a filename with extension specified should work.
                new[] { "followpoint.png" },
                "followpoint.png",
                "followpoint.png", 1
            },
            new object[]
            {
                // Looking up a filename with extension specified should also work with @2x sprites.
                new[] { "followpoint@2x.png" },
                "followpoint.png",
                "followpoint@2x.png", 2
            },
            new object[]
            {
                // Looking up a path with extension specified should work.
                new[] { "Gameplay/osu/followpoint.png" },
                "Gameplay/osu/followpoint.png",
                "Gameplay/osu/followpoint.png", 1
            },
            new object[]
            {
                // Looking up a path with extension specified should also work with @2x sprites.
                new[] { "Gameplay/osu/followpoint@2x.png" },
                "Gameplay/osu/followpoint.png",
                "Gameplay/osu/followpoint@2x.png", 2
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

        [Test]
        public void TestDisallowHighResolutionSprites()
        {
            var textureStore = new TestTextureStore("hitcircle", "hitcircle@2x");
            var legacySkin = new TestLegacySkin(textureStore) { HighResolutionSprites = false };

            var texture = legacySkin.GetTexture("hitcircle");

            Assert.IsNotNull(texture);
            Assert.That(texture.ScaleAdjust, Is.EqualTo(1));

            var twoTimesTexture = legacySkin.GetTexture("hitcircle@2x");

            Assert.IsNotNull(twoTimesTexture);
            Assert.That(twoTimesTexture.ScaleAdjust, Is.EqualTo(1));

            Assert.AreNotEqual(texture, twoTimesTexture);
        }

        [Test]
        public void TestAllowHighResolutionSprites()
        {
            var textureStore = new TestTextureStore("hitcircle", "hitcircle@2x");
            var legacySkin = new TestLegacySkin(textureStore) { HighResolutionSprites = true };

            var texture = legacySkin.GetTexture("hitcircle");

            Assert.IsNotNull(texture);
            Assert.That(texture.ScaleAdjust, Is.EqualTo(2));

            var twoTimesTexture = legacySkin.GetTexture("hitcircle@2x");

            Assert.IsNotNull(twoTimesTexture);
            Assert.That(twoTimesTexture.ScaleAdjust, Is.EqualTo(2));

            Assert.AreEqual(texture, twoTimesTexture);
        }

        private class TestLegacySkin : LegacySkin
        {
            public bool HighResolutionSprites { get; set; } = true;

            protected override bool AllowHighResolutionSprites => HighResolutionSprites;

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

                public IRenderer Renderer => new DummyRenderer();
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
                Textures = fileNames.ToDictionary(fileName => fileName, _ => new TextureUpload(new Image<Rgba32>(width, width++)));
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
