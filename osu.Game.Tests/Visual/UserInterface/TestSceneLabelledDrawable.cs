// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneLabelledDrawable : OsuTestScene
    {
        [TestCase(false)]
        [TestCase(true)]
        public void TestPadded(bool hasDescription) => createPaddedComponent(hasDescription);

        [TestCase(false)]
        [TestCase(true)]
        public void TestNonPadded(bool hasDescription) => createPaddedComponent(hasDescription, false);

        private void createPaddedComponent(bool hasDescription = false, bool padded = true)
        {
            AddStep("create component", () =>
            {
                LabelledDrawable<Drawable> component;

                Child = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 500,
                    AutoSizeAxes = Axes.Y,
                    Child = component = padded ? (LabelledDrawable<Drawable>)new PaddedLabelledDrawable() : new NonPaddedLabelledDrawable(),
                };

                component.Label = "a sample component";
                component.Description = hasDescription ? "this text describes the component" : string.Empty;
            });
        }

        private class PaddedLabelledDrawable : LabelledDrawable<Drawable>
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

        private class NonPaddedLabelledDrawable : LabelledDrawable<Drawable>
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
