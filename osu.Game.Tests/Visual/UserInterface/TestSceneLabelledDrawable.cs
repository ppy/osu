// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneLabelledDrawable : OsuTestScene
    {
        [TestCase(false)]
        [TestCase(true)]
        public void TestPadded(bool hasDescription) => createPaddedComponent(hasDescription);

        [TestCase(false)]
        [TestCase(true)]
        public void TestNonPadded(bool hasDescription) => createPaddedComponent(hasDescription, false);

        [Test]
        public void TestFixedWidth()
        {
            const float label_width = 200;

            AddStep("create components", () => Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10),
                Children = new Drawable[]
                {
                    new NonPaddedLabelledDrawable
                    {
                        Label = "short",
                        FixedLabelWidth = label_width
                    },
                    new NonPaddedLabelledDrawable
                    {
                        Label = "very very very very very very very very very very very long",
                        FixedLabelWidth = label_width
                    },
                    new PaddedLabelledDrawable
                    {
                        Label = "short",
                        FixedLabelWidth = label_width
                    },
                    new PaddedLabelledDrawable
                    {
                        Label = "very very very very very very very very very very very long",
                        FixedLabelWidth = label_width
                    }
                }
            });

            AddStep("unset label width", () => this.ChildrenOfType<LabelledDrawable<Drawable>>().ForEach(d => d.FixedLabelWidth = null));
            AddStep("reset label width", () => this.ChildrenOfType<LabelledDrawable<Drawable>>().ForEach(d => d.FixedLabelWidth = label_width));
        }

        private void createPaddedComponent(bool hasDescription = false, bool padded = true)
        {
            LabelledDrawable<Drawable> component = null;

            AddStep("create component", () =>
            {
                Child = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 500,
                    AutoSizeAxes = Axes.Y,
                    Child = component = padded ? new PaddedLabelledDrawable() : new NonPaddedLabelledDrawable(),
                };

                component.Label = "a sample component";
                component.Description = hasDescription ? "this text describes the component" : string.Empty;
            });

            AddAssert($"description {(hasDescription ? "visible" : "hidden")}", () => component.ChildrenOfType<TextFlowContainer>().ElementAt(1).IsPresent == hasDescription);
        }

        private partial class PaddedLabelledDrawable : LabelledDrawable<Drawable>
        {
            public PaddedLabelledDrawable()
                : base(true)
            {
            }

            protected override Drawable CreateComponent() => new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Colour = Color4.Red,
                Text = @"(( Component ))"
            };
        }

        private partial class NonPaddedLabelledDrawable : LabelledDrawable<Drawable>
        {
            public NonPaddedLabelledDrawable()
                : base(false)
            {
            }

            protected override Drawable CreateComponent() => new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = 40,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.SlateGray
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = Color4.Red,
                        Text = @"(( Component ))"
                    }
                }
            };
        }
    }
}
