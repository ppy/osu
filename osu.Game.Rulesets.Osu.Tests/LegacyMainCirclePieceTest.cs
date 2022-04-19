// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Rulesets.Osu.Skinning.Legacy;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;

#nullable enable

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
                Child = new DependencyProvidingContainer
                {
                    CachedDependencies = new (Type, object)[]
                    {
                        (typeof(ISkinSource), new TestSkin(textureFilenames))
                    },
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

        private class TestSkin : ISkinSource
        {
            private readonly string[] textureFilenames;

            public TestSkin(string[] textureFilenames)
            {
                this.textureFilenames = textureFilenames;
            }

            public Texture? GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
            {
                if (textureFilenames.Contains(componentName))
                    return new Texture(1, 1) { AssetName = componentName };

                return null;
            }

            public event Action SourceChanged
            {
                add { }
                remove { }
            }

            public IEnumerable<ISkin> AllSources { get; } = Enumerable.Empty<ISkin>();
            public Drawable? GetDrawableComponent(ISkinComponent component) => null;
            public ISample? GetSample(ISampleInfo sampleInfo) => null;

            public IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup)
                where TLookup : notnull
                where TValue : notnull
                => null;

            public ISkin? FindProvider(Func<ISkin, bool> lookupFunction) => lookupFunction(this) ? this : null;
        }
    }
}
