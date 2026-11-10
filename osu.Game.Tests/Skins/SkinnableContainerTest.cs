// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Tests.Skins
{
    [HeadlessTest]
    [TestFixture]
    public partial class SkinnableContainerTest : OsuTestScene
    {
        [Test]
        public void TestSkinnableConstraintsAreRespectedAtDeserialisationTime()
        {
            SkinnableContainer container = null!;

            AddStep("create container", () =>
            {
                Child = container = new SkinnableContainer(new GlobalSkinnableContainerLookup((GlobalSkinnableContainers)(-1)));
            });
            AddUntilStep("wait for loaded", () => container.ComponentsLoaded);

            AddStep("create components", () =>
            {
                container.Add(new SingletonSkinnable());
                container.Add(new SingletonSkinnable());
                container.Add(new ImmovableSkinnable { Position = new Vector2(300) });
                container.Add(new UnscaleableSkinnable { Scale = new Vector2(3) });
                container.Add(new NonrotatableSkinnable { Rotation = 42 });
            });

            AddStep("serialise and deserialise", () =>
            {
                var serialised = ((ISerialisableDrawableContainer)container).CreateSerialisedInfo();
                container.Reload(new Container
                {
                    ChildrenEnumerable = serialised.Select(s => s.CreateInstance())
                });
            });

            AddAssert("singleton constraint respected", () => container.Components.OfType<SingletonSkinnable>().Count(), () => Is.EqualTo(1));
            AddAssert("immovable constraint respected", () => container.Components.OfType<ImmovableSkinnable>().Single().Position, () => Is.EqualTo(Vector2.Zero));
            AddAssert("unscaleable constraint respected", () => container.Components.OfType<UnscaleableSkinnable>().Single().Scale, () => Is.EqualTo(Vector2.One));
            AddAssert("nonrotatable constraint respected", () => container.Components.OfType<NonrotatableSkinnable>().Single().Rotation, () => Is.Zero);
        }

        private partial class SingletonSkinnable : Drawable, ISerialisableDrawable
        {
            public bool UsesFixedAnchor { get; set; }
            public bool CanBePlaced(IEnumerable<ISerialisableDrawable> components) => !components.OfType<SingletonSkinnable>().Any();
        }

        private partial class ImmovableSkinnable : Drawable, ISerialisableDrawable
        {
            public bool UsesFixedAnchor { get; set; }
            public bool CanBeMoved => false;
        }

        private partial class UnscaleableSkinnable : Drawable, ISerialisableDrawable
        {
            public bool UsesFixedAnchor { get; set; }
            public bool CanBeScaled(Axes _) => false;
        }

        private partial class NonrotatableSkinnable : Drawable, ISerialisableDrawable
        {
            public bool UsesFixedAnchor { get; set; }
            public bool CanBeRotated => false;
        }
    }
}
