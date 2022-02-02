// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

namespace osu.Game.Tests.Gameplay
{
    [HeadlessTest]
    public class TestSceneHitObjectAccentColour : OsuTestScene
    {
        private Container skinContainer;

        [SetUp]
        public void Setup() => Schedule(() => Child = skinContainer = new SkinProvidingContainer(new TestSkin()));

        [Test]
        public void TestChangeComboIndexBeforeLoad()
        {
            TestDrawableHitObject hitObject = null;

            AddStep("set combo and add hitobject", () =>
            {
                hitObject = new TestDrawableHitObject();
                hitObject.HitObject.ComboIndex = 1;

                skinContainer.Add(hitObject);
            });

            AddAssert("combo colour is green", () => hitObject.AccentColour.Value == Color4.Green);
        }

        [Test]
        public void TestChangeComboIndexDuringLoad()
        {
            TestDrawableHitObject hitObject = null;

            AddStep("add hitobject and set combo", () =>
            {
                skinContainer.Add(hitObject = new TestDrawableHitObject());
                hitObject.HitObject.ComboIndex = 1;
            });

            AddAssert("combo colour is green", () => hitObject.AccentColour.Value == Color4.Green);
        }

        [Test]
        public void TestChangeComboIndexAfterLoad()
        {
            TestDrawableHitObject hitObject = null;

            AddStep("add hitobject", () => skinContainer.Add(hitObject = new TestDrawableHitObject()));
            AddAssert("combo colour is red", () => hitObject.AccentColour.Value == Color4.Red);

            AddStep("change combo", () => hitObject.HitObject.ComboIndex = 1);
            AddAssert("combo colour is green", () => hitObject.AccentColour.Value == Color4.Green);
        }

        private class TestDrawableHitObject : DrawableHitObject<TestHitObjectWithCombo>
        {
            public TestDrawableHitObject()
                : base(new TestHitObjectWithCombo())
            {
            }
        }

        private class TestHitObjectWithCombo : ConvertHitObject, IHasComboInformation
        {
            public bool NewCombo { get; set; }
            public int ComboOffset => 0;

            public Bindable<int> IndexInCurrentComboBindable { get; } = new Bindable<int>();

            public int IndexInCurrentCombo
            {
                get => IndexInCurrentComboBindable.Value;
                set => IndexInCurrentComboBindable.Value = value;
            }

            public Bindable<int> ComboIndexBindable { get; } = new Bindable<int>();

            public int ComboIndex
            {
                get => ComboIndexBindable.Value;
                set => ComboIndexBindable.Value = value;
            }

            public Bindable<int> ComboIndexWithOffsetsBindable { get; } = new Bindable<int>();

            public int ComboIndexWithOffsets
            {
                get => ComboIndexWithOffsetsBindable.Value;
                set => ComboIndexWithOffsetsBindable.Value = value;
            }

            public Bindable<bool> LastInComboBindable { get; } = new Bindable<bool>();

            public bool LastInCombo
            {
                get => LastInComboBindable.Value;
                set => LastInComboBindable.Value = value;
            }
        }

        private class TestSkin : ISkin
        {
            public readonly List<Color4> ComboColours = new List<Color4>
            {
                Color4.Red,
                Color4.Green
            };

            public Drawable GetDrawableComponent(ISkinComponent component) => throw new NotImplementedException();

            public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => throw new NotImplementedException();

            public ISample GetSample(ISampleInfo sampleInfo) => throw new NotImplementedException();

            public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
            {
                switch (lookup)
                {
                    case SkinComboColourLookup comboColour:
                        return SkinUtils.As<TValue>(new Bindable<Color4>(ComboColours[comboColour.ColourIndex % ComboColours.Count]));
                }

                throw new NotImplementedException();
            }
        }
    }
}
