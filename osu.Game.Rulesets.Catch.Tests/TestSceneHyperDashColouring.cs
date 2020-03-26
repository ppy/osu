// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.Skinning;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Tests
{
    public class TestSceneHyperDashColouring : OsuTestScene
    {
        private Drawable setupSkinHierarchy(Func<Drawable> getChild, bool customHyperDashCatcherColour = false, bool customHyperDashFruitColour = false, bool customHyperDashAfterColour = false)
        {
            var testSkinProvider = new SkinProvidingContainer(new TestLegacySkin(customHyperDashCatcherColour, customHyperDashFruitColour, customHyperDashAfterColour));

            var legacySkinTransformer = new SkinProvidingContainer(new CatchLegacySkinTransformer(testSkinProvider));

            return testSkinProvider
                .WithChild(legacySkinTransformer
                    .WithChild(getChild.Invoke()));
        }

        private bool checkFruitHyperDashColour(DrawableFruit fruit, Color4 expectedColour, bool isLegacyFruit) =>
            isLegacyFruit
                ? fruit.ChildrenOfType<SkinnableDrawable>().First().Drawable.ChildrenOfType<Sprite>().Any(c => c.Colour == expectedColour)
                : fruit.ChildrenOfType<SkinnableDrawable>().First().Drawable.ChildrenOfType<Circle>().Single(c => c.BorderColour == expectedColour).Any(d => d.Colour == expectedColour);

        private class TestLegacySkin : ISkin
        {
            public static Color4 CustomHyperDashColour { get; } = Color4.Goldenrod;
            public static Color4 CustomHyperDashFruitColour { get; } = Color4.Cyan;
            public static Color4 CustomHyperDashAfterColour { get; } = Color4.Lime;

            private readonly bool customHyperDashCatcherColour;
            private readonly bool customHyperDashFruitColour;
            private readonly bool customHyperDashAfterColour;

            public TestLegacySkin(bool customHyperDashCatcherColour = false, bool customHyperDashFruitColour = false, bool customHyperDashAfterColour = false)
            {
                this.customHyperDashCatcherColour = customHyperDashCatcherColour;
                this.customHyperDashFruitColour = customHyperDashFruitColour;
                this.customHyperDashAfterColour = customHyperDashAfterColour;
            }

            public Drawable GetDrawableComponent(ISkinComponent component) => null;

            public Texture GetTexture(string componentName)
            {
                if (componentName == "fruit-pear")
                {
                    // convince CatchLegacySkinTransformer to use the LegacyFruitPiece for pear fruit.
                    var texture = new Texture(Texture.WhitePixel.TextureGL)
                    {
                        Width = 1,
                        Height = 1,
                        ScaleAdjust = 1 / 96f
                    };
                    return texture;
                }

                return null;
            }

            public SampleChannel GetSample(ISampleInfo sampleInfo) => null;

            public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
            {
                switch (lookup)
                {
                    case CatchSkinConfiguration config when config == CatchSkinConfiguration.HyperDash:
                        if (customHyperDashCatcherColour)
                            return SkinUtils.As<TValue>(new Bindable<Color4>(CustomHyperDashColour));

                        return null;

                    case CatchSkinConfiguration config when config == CatchSkinConfiguration.HyperDashFruit:
                        if (customHyperDashFruitColour)
                            return SkinUtils.As<TValue>(new Bindable<Color4>(CustomHyperDashFruitColour));

                        return null;

                    case CatchSkinConfiguration config when config == CatchSkinConfiguration.HyperDashAfterImage:
                        if (customHyperDashAfterColour)
                            return SkinUtils.As<TValue>(new Bindable<Color4>(CustomHyperDashAfterColour));

                        return null;
                }

                return null;
            }
        }
    }
}
