// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
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
                Anchor = Anchor.Centre
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
            AddStep("add third", () => append(container.ChildSize.Y / 3.0f));
            AddStep("add half", () => append(container.ChildSize.Y / 2.0f));
            AddStep("add full", () => append(container.ChildSize.Y));
            AddSliderStep("set custom", 0.1f, 1.1f, 0.5f, i => custom = i);
            AddStep("add custom", () => append(container.ChildSize.Y * custom));
        }

        private TestSection append(float height)
        {
            var rv = new TestSection
            {
                Width = 300,
                Height = height,
                Margin = new MarginPadding { Top = 10 },
                Colour = Color4.Gray
            };
            container.Add(rv);
            return rv;
        }

        private class TestSection : Box
        {
            private bool selected;

            public bool Selected
            {
                get => selected;
                set
                {
                    selected = value;
                    Colour = selected ? Color4.Yellow : Color4.Gray;
                }
            }
        }
    }
}
