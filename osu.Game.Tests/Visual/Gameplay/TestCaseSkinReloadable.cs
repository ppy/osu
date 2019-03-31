// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestCaseSkinReloadable : OsuTestCase
    {
        [Test]
        public void TestInitialLoad()
        {
            var secondarySource = new SecondarySource();
            SkinConsumer consumer = null;

            AddStep("setup layout", () =>
            {
                Child = new SkinSourceContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new LocalSkinOverrideContainer(secondarySource)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = consumer = new SkinConsumer("test", name => new NamedBox("Default Implementation"), source => true)
                    }
                };
            });

            AddAssert("consumer using override source", () => consumer.Drawable is SecondarySourceBox);
            AddAssert("skinchanged only called once", () => consumer.SkinChangedCount == 1);
        }

        [Test]
        public void TestOverride()
        {
            var secondarySource = new SecondarySource();

            SkinConsumer consumer = null;
            Container target = null;

            AddStep("setup layout", () =>
            {
                Child = new SkinSourceContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = target = new LocalSkinOverrideContainer(secondarySource)
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                };
            });

            AddStep("add permissive", () => target.Add(consumer = new SkinConsumer("test", name => new NamedBox("Default Implementation"), source => true)));
            AddAssert("consumer using override source", () => consumer.Drawable is SecondarySourceBox);
            AddAssert("skinchanged only called once", () => consumer.SkinChangedCount == 1);
        }

        private class NamedBox : Container
        {
            public NamedBox(string name)
            {
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = Color4.Black,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new SpriteText
                    {
                        Font = OsuFont.Default.With(size: 40),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = name
                    }
                };
            }
        }

        private class SkinConsumer : SkinnableDrawable
        {
            public new Drawable Drawable => base.Drawable;
            public int SkinChangedCount { get; private set; }

            public SkinConsumer(string name, Func<string, Drawable> defaultImplementation, Func<ISkinSource, bool> allowFallback = null, bool restrictSize = true)
                : base(name, defaultImplementation, allowFallback, restrictSize)
            {
            }

            protected override void SkinChanged(ISkinSource skin, bool allowFallback)
            {
                base.SkinChanged(skin, allowFallback);
                SkinChangedCount++;
            }
        }

        private class BaseSourceBox : NamedBox
        {
            public BaseSourceBox()
                : base("Base Source")
            {
            }
        }

        private class SecondarySourceBox : NamedBox
        {
            public SecondarySourceBox()
                : base("Secondary Source")
            {
            }
        }

        private class SecondarySource : ISkinSource
        {
            public event Action SourceChanged;

            public void TriggerSourceChanged() => SourceChanged?.Invoke();

            public Drawable GetDrawableComponent(string componentName) => new SecondarySourceBox();

            public Texture GetTexture(string componentName) => throw new NotImplementedException();

            public SampleChannel GetSample(string sampleName) => throw new NotImplementedException();

            public TValue GetValue<TConfiguration, TValue>(Func<TConfiguration, TValue> query) where TConfiguration : SkinConfiguration => throw new NotImplementedException();
        }

        private class SkinSourceContainer : Container, ISkinSource
        {
            public event Action SourceChanged;

            public void TriggerSourceChanged() => SourceChanged?.Invoke();

            public Drawable GetDrawableComponent(string componentName) => new BaseSourceBox();

            public Texture GetTexture(string componentName) => throw new NotImplementedException();

            public SampleChannel GetSample(string sampleName) => throw new NotImplementedException();

            public TValue GetValue<TConfiguration, TValue>(Func<TConfiguration, TValue> query) where TConfiguration : SkinConfiguration => throw new NotImplementedException();
        }
    }
}
