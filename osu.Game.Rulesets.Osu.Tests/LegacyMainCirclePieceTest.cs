// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using Moq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Game.Rulesets.Osu.Skinning.Legacy;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests
{
    [HeadlessTest]
    public partial class LegacyMainCirclePieceTest : OsuTestScene
    {
        [Resolved]
        private IRenderer renderer { get; set; } = null!;

        private static readonly object?[][] texture_priority_cases =
        {
            // default priority lookup
            new object?[]
            {
                // available textures
                new[] { @"hitcircle", @"hitcircleoverlay" },
                // priority lookup prefix
                null,
                // expected circle and overlay
                @"hitcircle", @"hitcircleoverlay",
            },
            // custom priority lookup
            new object?[]
            {
                new[] { @"hitcircle", @"hitcircleoverlay", @"sliderstartcircle", @"sliderstartcircleoverlay" },
                @"sliderstartcircle",
                @"sliderstartcircle", @"sliderstartcircleoverlay",
            },
            // when no sprites are available for the specified prefix, fall back to "hitcircle"/"hitcircleoverlay".
            new object?[]
            {
                new[] { @"hitcircle", @"hitcircleoverlay" },
                @"sliderstartcircle",
                @"hitcircle", @"hitcircleoverlay",
            },
            // when a circle is available for the specified prefix but no overlay exists, no overlay is displayed.
            new object?[]
            {
                new[] { @"hitcircle", @"hitcircleoverlay", @"sliderstartcircle" },
                @"sliderstartcircle",
                @"sliderstartcircle", null
            },
            // when no circle is available for the specified prefix but an overlay exists, the overlay is ignored.
            new object?[]
            {
                new[] { @"hitcircle", @"hitcircleoverlay", @"sliderstartcircleoverlay" },
                @"sliderstartcircle",
                @"hitcircle", @"hitcircleoverlay",
            }
        };

        [TestCaseSource(nameof(texture_priority_cases))]
        public void TestTexturePriorities(string[] textureFilenames, string priorityLookup, string? expectedCircle, string? expectedOverlay)
        {
            TestLegacyMainCirclePiece piece = null!;

            AddStep("load circle piece", () =>
            {
                var skin = new Mock<ISkinSource>();

                // shouldn't be required as GetTexture(string) calls GetTexture(string, WrapMode, WrapMode) by default,
                // but moq doesn't handle that well, therefore explicitly requiring to use `CallBase`:
                // https://github.com/moq/moq4/issues/972
                skin.Setup(s => s.GetTexture(It.IsAny<string>())).CallBase();

                skin.Setup(s => s.GetTexture(It.IsIn(textureFilenames), It.IsAny<WrapMode>(), It.IsAny<WrapMode>()))
                    .Returns((string componentName, WrapMode _, WrapMode _) =>
                    {
                        var tex = renderer.CreateTexture(1, 1);
                        tex.AssetName = componentName;
                        return tex;
                    });

                Child = new DependencyProvidingContainer
                {
                    CachedDependencies = new (Type, object)[] { (typeof(ISkinSource), skin.Object) },
                    Child = piece = new TestLegacyMainCirclePiece(priorityLookup),
                };

                var sprites = this.ChildrenOfType<Sprite>().Where(s => !string.IsNullOrEmpty(s.Texture?.AssetName)).DistinctBy(s => s.Texture.AssetName).ToArray();
                Debug.Assert(sprites.Length <= 2);
            });

            AddAssert("check circle sprite", () => piece.CircleSprite?.Texture?.AssetName == expectedCircle);
            AddAssert("check overlay sprite", () => piece.OverlaySprite?.Texture?.AssetName == expectedOverlay);
        }

        private partial class TestLegacyMainCirclePiece : LegacyMainCirclePiece
        {
            public new Sprite? CircleSprite => base.CircleSprite.ChildrenOfType<Sprite>().Where(s => !string.IsNullOrEmpty(s.Texture?.AssetName)).DistinctBy(s => s.Texture.AssetName).SingleOrDefault();
            public new Sprite? OverlaySprite => base.OverlaySprite.ChildrenOfType<Sprite>().Where(s => !string.IsNullOrEmpty(s.Texture?.AssetName)).DistinctBy(s => s.Texture.AssetName).SingleOrDefault();

            public TestLegacyMainCirclePiece(string? priorityLookupPrefix)
                : base(priorityLookupPrefix, false)
            {
            }
        }
    }
}
