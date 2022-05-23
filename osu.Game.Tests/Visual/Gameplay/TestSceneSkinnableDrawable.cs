// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSkinnableDrawable : OsuTestScene
    {
        [Test]
        public void TestConfineScaleDown()
        {
            FillFlowContainer<ExposedSkinnableDrawable> fill = null;

            AddStep("setup layout larger source", () =>
            {
                Child = new SkinProvidingContainer(new SizedSource(50))
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = fill = new FillFlowContainer<ExposedSkinnableDrawable>
                    {
                        Size = new Vector2(30),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Spacing = new Vector2(10),
                        Children = new[]
                        {
                            new ExposedSkinnableDrawable("default", _ => new DefaultBox()),
                            new ExposedSkinnableDrawable("available", _ => new DefaultBox()),
                            new ExposedSkinnableDrawable("available", _ => new DefaultBox(), ConfineMode.NoScaling)
                        }
                    },
                };
            });

            AddAssert("check sizes", () => fill.Children.Select(c => c.Drawable.DrawWidth).SequenceEqual(new float[] { 30, 30, 50 }));
            AddStep("adjust scale", () => fill.Scale = new Vector2(2));
            AddAssert("check sizes unchanged by scale", () => fill.Children.Select(c => c.Drawable.DrawWidth).SequenceEqual(new float[] { 30, 30, 50 }));
        }

        [Test]
        public void TestConfineScaleUp()
        {
            FillFlowContainer<ExposedSkinnableDrawable> fill = null;

            AddStep("setup layout larger source", () =>
            {
                Child = new SkinProvidingContainer(new SizedSource(30))
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = fill = new FillFlowContainer<ExposedSkinnableDrawable>
                    {
                        Size = new Vector2(50),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Spacing = new Vector2(10),
                        Children = new[]
                        {
                            new ExposedSkinnableDrawable("default", _ => new DefaultBox()),
                            new ExposedSkinnableDrawable("available", _ => new DefaultBox()),
                            new ExposedSkinnableDrawable("available", _ => new DefaultBox(), ConfineMode.NoScaling)
                        }
                    },
                };
            });

            AddAssert("check sizes", () => fill.Children.Select(c => c.Drawable.DrawWidth).SequenceEqual(new float[] { 50, 50, 30 }));
            AddStep("adjust scale", () => fill.Scale = new Vector2(2));
            AddAssert("check sizes unchanged by scale", () => fill.Children.Select(c => c.Drawable.DrawWidth).SequenceEqual(new float[] { 50, 50, 30 }));
        }

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
                    Child = new SkinProvidingContainer(secondarySource)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = consumer = new SkinConsumer("test", name => new NamedBox("Default Implementation"))
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
                    Child = target = new SkinProvidingContainer(secondarySource)
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                };
            });

            AddStep("add permissive", () => target.Add(consumer = new SkinConsumer("test", name => new NamedBox("Default Implementation"))));
            AddAssert("consumer using override source", () => consumer.Drawable is SecondarySourceBox);
            AddAssert("skinchanged only called once", () => consumer.SkinChangedCount == 1);
        }

        [Test]
        public void TestSwitchOff()
        {
            SkinConsumer consumer = null;
            SwitchableSkinProvidingContainer target = null;

            AddStep("setup layout", () =>
            {
                Child = new SkinSourceContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = target = new SwitchableSkinProvidingContainer(new SecondarySource())
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                };
            });

            AddStep("add permissive", () => target.Add(consumer = new SkinConsumer("test", name => new NamedBox("Default Implementation"))));
            AddAssert("consumer using override source", () => consumer.Drawable is SecondarySourceBox);
            AddStep("disable", () => target.Disable());
            AddAssert("consumer using base source", () => consumer.Drawable is BaseSourceBox);
        }

        private class SwitchableSkinProvidingContainer : SkinProvidingContainer
        {
            private bool allow = true;

            protected override bool AllowDrawableLookup(ISkinComponent component) => allow;

            public void Disable()
            {
                allow = false;
                TriggerSourceChanged();
            }

            public SwitchableSkinProvidingContainer(ISkin skin)
                : base(skin)
            {
            }
        }

        private class ExposedSkinnableDrawable : SkinnableDrawable
        {
            public new Drawable Drawable => base.Drawable;

            public ExposedSkinnableDrawable(string name, Func<ISkinComponent, Drawable> defaultImplementation, ConfineMode confineMode = ConfineMode.ScaleToFit)
                : base(new TestSkinComponent(name), defaultImplementation, confineMode)
            {
            }
        }

        private class DefaultBox : DrawWidthBox
        {
            public DefaultBox()
            {
                RelativeSizeAxes = Axes.Both;
            }
        }

        private class DrawWidthBox : Container
        {
            private readonly OsuSpriteText text;

            public DrawWidthBox()
            {
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = Color4.Gray,
                        RelativeSizeAxes = Axes.Both,
                    },
                    text = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                };
            }

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();
                text.Text = DrawWidth.ToString(CultureInfo.InvariantCulture);
            }
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
                    new OsuSpriteText
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

            public SkinConsumer(string name, Func<ISkinComponent, Drawable> defaultImplementation)
                : base(new TestSkinComponent(name), defaultImplementation)
            {
            }

            protected override void SkinChanged(ISkinSource skin)
            {
                base.SkinChanged(skin);
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

        private class SizedSource : ISkin
        {
            private readonly float size;

            public SizedSource(float size)
            {
                this.size = size;
            }

            public Drawable GetDrawableComponent(ISkinComponent componentName) =>
                componentName.LookupName == "available"
                    ? new DrawWidthBox
                    {
                        Colour = Color4.Yellow,
                        Size = new Vector2(size)
                    }
                    : null;

            public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => throw new NotImplementedException();

            public ISample GetSample(ISampleInfo sampleInfo) => throw new NotImplementedException();

            public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => throw new NotImplementedException();
        }

        private class SecondarySource : ISkin
        {
            public Drawable GetDrawableComponent(ISkinComponent componentName) => new SecondarySourceBox();

            public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => throw new NotImplementedException();

            public ISample GetSample(ISampleInfo sampleInfo) => throw new NotImplementedException();

            public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => throw new NotImplementedException();
        }

        [Cached(typeof(ISkinSource))]
        private class SkinSourceContainer : Container, ISkinSource
        {
            public Drawable GetDrawableComponent(ISkinComponent componentName) => new BaseSourceBox();

            public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => throw new NotImplementedException();

            public ISample GetSample(ISampleInfo sampleInfo) => throw new NotImplementedException();

            public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => throw new NotImplementedException();

            public ISkin FindProvider(Func<ISkin, bool> lookupFunction) => throw new NotImplementedException();

            public IEnumerable<ISkin> AllSources => throw new NotImplementedException();

            public event Action SourceChanged
            {
                add { }
                remove { }
            }
        }

        private class TestSkinComponent : ISkinComponent
        {
            public TestSkinComponent(string name)
            {
                LookupName = name;
            }

            public string ComponentGroup => string.Empty;

            public string LookupName { get; }
        }
    }
}
