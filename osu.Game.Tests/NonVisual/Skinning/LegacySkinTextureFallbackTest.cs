// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Game.Skinning;

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
            Assert.AreEqual(textureStore.Textures[expectedTexture], texture);
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
            public TestLegacySkin(TextureStore textureStore)
                : base(new SkinInfo(), null, null, string.Empty)
            {
                Textures = textureStore;
            }
        }

        private class TestTextureStore : TextureStore
        {
            public readonly Dictionary<string, Texture> Textures;

            public TestTextureStore(params string[] fileNames)
            {
                Textures = fileNames.ToDictionary(fileName => fileName, fileName => new Texture(1, 1));
            }

            public override Texture Get(string name, WrapMode wrapModeS, WrapMode wrapModeT) => Textures.GetValueOrDefault(name);
        }
    }
}
