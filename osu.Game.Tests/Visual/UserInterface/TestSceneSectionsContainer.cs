// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneSectionsContainer : OsuTestScene
    {
        private readonly SectionsContainer<TestSection> container;
        private TestSection selectedSection;
        private float custom;

        public TestSceneSectionsContainer()
        {
            container = new SectionsContainer<TestSection>
            {
                RelativeSizeAxes = Axes.Y,
                Width = 300,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                FixedHeader = new Box
                {
                    Alpha = 0.5f,
                    Width = 300,
                    Height = 100,
                    Colour = Color4.Red
                }
            };
            container.SelectedSection.ValueChanged += section =>
            {
                if (selectedSection != null)
                    selectedSection.Selected = false;
                selectedSection = section.NewValue;
                if (selectedSection != null)
                    selectedSection.Selected = true;
            };
            Add(container);
        }

        [Test]
        public void TestSelection()
        {
            AddStep("clear", () => container.Clear());
            AddStep("add 1/8th", () => append(container.ChildSize.Y / 8.0f));
            AddStep("add third", () => append(container.ChildSize.Y / 3.0f));
            AddStep("add half", () => append(container.ChildSize.Y / 2.0f));
            AddStep("add full", () => append(container.ChildSize.Y));
            AddSliderStep("set custom", 0.1f, 1.1f, 0.5f, i => custom = i);
            AddStep("add custom", () => append(container.ChildSize.Y * custom));
            AddStep("scroll to next", () => container.ScrollTo(container.Children.SkipWhile(s => s != container.SelectedSection.Value).Skip(1).FirstOrDefault()));
            AddStep("scroll to previous", () => container.ScrollTo(
                container.Children.Reverse().SkipWhile(s => s != container.SelectedSection.Value).Skip(1).FirstOrDefault()
            ));
        }

        private static readonly ColourInfo selected_colour = ColourInfo.GradientVertical(Color4.Yellow, Color4.Gold);
        private static readonly ColourInfo default_colour = ColourInfo.GradientVertical(Color4.White, Color4.DarkGray);

        private TestSection append(float height)
        {
            var rv = new TestSection
            {
                Width = 300,
                Height = height,
                Colour = default_colour
            };
            container.Add(rv);
            return rv;
        }

        private class TestSection : Box
        {
            public bool Selected
            {
                set => Colour = value ? selected_colour : default_colour;
            }
        }
    }
}
