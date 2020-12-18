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
    public class TestSceneSectionsContainer : OsuManualInputManagerTestScene
    {
        private readonly SectionsContainer<TestSection> container;
        private float custom;
        private const float header_height = 100;

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
                    Height = header_height,
                    Colour = Color4.Red
                }
            };
            container.SelectedSection.ValueChanged += section =>
            {
                if (section.OldValue != null)
                    section.OldValue.Selected = false;
                if (section.NewValue != null)
                    section.NewValue.Selected = true;
            };
            Add(container);
        }

        [Test]
        public void TestSelection()
        {
            AddStep("clear", () => container.Clear());
            AddStep("add 1/8th", () => append(1 / 8.0f));
            AddStep("add third", () => append(1 / 3.0f));
            AddStep("add half", () => append(1 / 2.0f));
            AddStep("add full", () => append(1));
            AddSliderStep("set custom", 0.1f, 1.1f, 0.5f, i => custom = i);
            AddStep("add custom", () => append(custom));
            AddStep("scroll to previous", () => container.ScrollTo(
                container.Children.Reverse().SkipWhile(s => s != container.SelectedSection.Value).Skip(1).FirstOrDefault() ?? container.Children.First()
            ));
            AddStep("scroll to next", () => container.ScrollTo(
                container.Children.SkipWhile(s => s != container.SelectedSection.Value).Skip(1).FirstOrDefault() ?? container.Children.Last()
            ));
            AddStep("scroll up", () => triggerUserScroll(1));
            AddStep("scroll down", () => triggerUserScroll(-1));
        }

        [Test]
        public void TestCorrectSectionSelected()
        {
            const int sections_count = 11;
            float[] alternating = { 0.07f, 0.33f, 0.16f, 0.33f };
            AddStep("clear", () => container.Clear());
            AddStep("fill with sections", () =>
            {
                for (int i = 0; i < sections_count; i++)
                    append(alternating[i % alternating.Length]);
            });

            void step(int scrollIndex)
            {
                AddStep($"scroll to section {scrollIndex + 1}", () => container.ScrollTo(container.Children[scrollIndex]));
                AddUntilStep("correct section selected", () => container.SelectedSection.Value == container.Children[scrollIndex]);
            }

            for (int i = 1; i < sections_count; i++)
                step(i);
            for (int i = sections_count - 2; i >= 0; i--)
                step(i);

            AddStep("scroll almost to end", () => container.ScrollTo(container.Children[sections_count - 2]));
            AddUntilStep("correct section selected", () => container.SelectedSection.Value == container.Children[sections_count - 2]);
            AddStep("scroll down", () => triggerUserScroll(-1));
            AddUntilStep("correct section selected", () => container.SelectedSection.Value == container.Children[sections_count - 1]);
        }

        private static readonly ColourInfo selected_colour = ColourInfo.GradientVertical(Color4.Yellow, Color4.Gold);
        private static readonly ColourInfo default_colour = ColourInfo.GradientVertical(Color4.White, Color4.DarkGray);

        private void append(float multiplier)
        {
            container.Add(new TestSection
            {
                Width = 300,
                Height = (container.ChildSize.Y - header_height) * multiplier,
                Colour = default_colour
            });
        }

        private void triggerUserScroll(float direction)
        {
            InputManager.MoveMouseTo(container);
            InputManager.ScrollVerticalBy(direction);
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
