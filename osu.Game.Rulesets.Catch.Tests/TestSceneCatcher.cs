// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Tests.Visual;
using System;
using System.Collections.Generic;
using osu.Game.Skinning;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK.Graphics;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestSceneCatcher : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(CatcherSprite),
        };

        private readonly Container container;

        public TestSceneCatcher()
        {
            Child = container = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddStep("show default catcher implementation", () => { container.Child = new CatcherSprite(); });

            AddStep("show custom catcher implementation", () =>
            {
                container.Child = new CatchCustomSkinSourceContainer
                {
                    Child = new CatcherSprite()
                };
            });
        }

        private class CatcherCustomSkin : Container
        {
            public CatcherCustomSkin()
            {
                RelativeSizeAxes = Axes.Both;

                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Blue
                    },
                    new SpriteText
                    {
                        Text = "custom"
                    }
                };
            }
        }

        [Cached(typeof(ISkinSource))]
        private class CatchCustomSkinSourceContainer : Container, ISkinSource
        {
            public event Action SourceChanged
            {
                add { }
                remove { }
            }

            public Drawable GetDrawableComponent(ISkinComponent component)
            {
                switch (component.LookupName)
                {
                    case "Gameplay/catch/fruit-catcher-idle":
                        return new CatcherCustomSkin();
                }

                return null;
            }

            public SampleChannel GetSample(ISampleInfo sampleInfo) =>
                throw new NotImplementedException();

            public Texture GetTexture(string componentName) =>
                throw new NotImplementedException();

            public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => throw new NotImplementedException();
        }
    }
}
