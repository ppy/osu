// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Diagnostics;
using System.Linq;
using Moq;
using NUnit.Framework;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Game.Rulesets.Osu.Skinning.Legacy;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests
{
    [HeadlessTest]
    public class LegacyMainCirclePieceTest : OsuTestScene
    {
        private static readonly object?[][] texture_priority_cases =
        {
            // default priority lookup
            new object?[]
            {
                // available textures
                new[] { @"hitcircle", @"hitcircleoverlay" },
                // priority lookup
                @"",
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
            Sprite? circleSprite = null;
            Sprite? overlaySprite = null;

            AddStep("load circle piece", () =>
            {
                var skin = new Mock<ISkinSource>();

                skin.Setup(s => s.GetTexture(It.IsAny<string>())).CallBase();
                skin.Setup(s => s.GetTexture(It.IsIn(textureFilenames), It.IsAny<WrapMode>(), It.IsAny<WrapMode>()))
                    .Returns((string componentName, WrapMode _, WrapMode __) => new Texture(1, 1) { AssetName = componentName });

                Child = new DependencyProvidingContainer
                {
                    CachedDependencies = new (Type, object)[] { (typeof(ISkinSource), skin.Object) },
                    Child = new LegacyMainCirclePiece(priorityLookup, false),
                };

                var sprites = this.ChildrenOfType<Sprite>().Where(s => s.Texture.AssetName != null).DistinctBy(s => s.Texture.AssetName).ToArray();
                Debug.Assert(sprites.Length <= 2);

                circleSprite = sprites.ElementAtOrDefault(0);
                overlaySprite = sprites.ElementAtOrDefault(1);
            });

            AddAssert("check circle sprite", () => circleSprite?.Texture?.AssetName == expectedCircle);
            AddAssert("check overlay sprite", () => overlaySprite?.Texture?.AssetName == expectedOverlay);
        }
    }
}
