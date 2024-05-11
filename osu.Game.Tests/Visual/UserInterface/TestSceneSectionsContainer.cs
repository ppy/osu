// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneSectionsContainer : OsuManualInputManagerTestScene
    {
        private SectionsContainer<TestSection> container;
        private float custom;

        private const float header_expandable_height = 300;
        private const float header_fixed_height = 100;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("setup container", () =>
            {
                container = new SectionsContainer<TestSection>
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 300,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                };

                container.SelectedSection.ValueChanged += section =>
                {
                    if (section.OldValue != null)
                        section.OldValue.Selected = false;
                    if (section.NewValue != null)
                        section.NewValue.Selected = true;
                };

                Child = container;
            });

            AddToggleStep("disable expandable header", v => container.ExpandableHeader = v
                ? null
                : new TestBox(@"Expandable Header")
                {
                    RelativeSizeAxes = Axes.X,
                    Height = header_expandable_height,
                    BackgroundColour = new OsuColour().GreySky,
                });

            AddToggleStep("disable fixed header", v => container.FixedHeader = v
                ? null
                : new TestBox(@"Fixed Header")
                {
                    RelativeSizeAxes = Axes.X,
                    Height = header_fixed_height,
                    BackgroundColour = new OsuColour().Red.Opacity(0.5f),
                });

            AddToggleStep("disable footer", v => container.Footer = v
                ? null
                : new TestBox("Footer")
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 200,
                    BackgroundColour = new OsuColour().Green4,
                });
        }

        [Test]
        public void TestCorrectScrollToWhenContentLoads()
        {
            AddRepeatStep("add many sections", () => append(1f), 3);

            AddStep("add section with delayed load content", () =>
            {
                container.Add(new TestDelayedLoadSection("delayed"));
            });

            AddStep("add final section", () => append(0.5f));

            AddStep("scroll to final section", () => container.ScrollTo(container.Children.Last()));

            AddUntilStep("correct section selected", () => container.SelectedSection.Value == container.Children.Last());
            AddUntilStep("wait for scroll to section", () => container.ScreenSpaceDrawQuad.AABBFloat.Contains(container.Children.Last().ScreenSpaceDrawQuad.AABBFloat));
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
            AddStep("scroll up a bit", () => triggerUserScroll(0.1f));
            AddStep("scroll down a bit", () => triggerUserScroll(-0.1f));
        }

        [Test]
        public void TestCorrectSelectionAndVisibleTop()
        {
            const int sections_count = 11;
            float[] alternating = { 0.07f, 0.33f, 0.16f, 0.33f };
            AddStep("fill with sections", () =>
            {
                for (int i = 0; i < sections_count; i++)
                    append(alternating[i % alternating.Length]);
            });

            void step(int scrollIndex)
            {
                AddStep($"scroll to section {scrollIndex + 1}", () => container.ScrollTo(container.Children[scrollIndex]));
                AddUntilStep("correct section selected", () => container.SelectedSection.Value == container.Children[scrollIndex]);
                AddUntilStep("section top is visible", () =>
                {
                    var scrollContainer = container.ChildrenOfType<UserTrackingScrollContainer>().Single();
                    float sectionPosition = scrollContainer.GetChildPosInContent(container.Children[scrollIndex]);
                    return scrollContainer.Current < sectionPosition;
                });
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

        [Test]
        public void TestNavigation()
        {
            AddRepeatStep("add sections", () => append(1f), 3);
            AddUntilStep("wait for load", () => container.Children.Any());

            AddStep("hover sections container", () => InputManager.MoveMouseTo(container));
            AddStep("press page down", () => InputManager.Key(Key.PageDown));
            AddUntilStep("scrolled one page down", () =>
            {
                var scroll = container.ChildrenOfType<UserTrackingScrollContainer>().First();
                return Precision.AlmostEquals(scroll.Current, Content.DrawHeight - header_fixed_height, 1f);
            });

            AddStep("press page down", () => InputManager.Key(Key.PageDown));
            AddUntilStep("scrolled two pages down", () =>
            {
                var scroll = container.ChildrenOfType<UserTrackingScrollContainer>().First();
                return Precision.AlmostEquals(scroll.Current, (Content.DrawHeight - header_fixed_height) * 2, 1f);
            });

            AddStep("press page up", () => InputManager.Key(Key.PageUp));
            AddUntilStep("scrolled one page up", () =>
            {
                var scroll = container.ChildrenOfType<UserTrackingScrollContainer>().First();
                return Precision.AlmostEquals(scroll.Current, Content.DrawHeight - header_fixed_height, 1f);
            });
        }

        private static readonly ColourInfo selected_colour = ColourInfo.GradientVertical(new OsuColour().Orange2, new OsuColour().Orange3);
        private static readonly ColourInfo default_colour = ColourInfo.GradientVertical(Color4.White, Color4.DarkGray);

        private void append(float multiplier)
        {
            float fixedHeaderHeight = container.FixedHeader?.Height ?? 0;
            float expandableHeaderHeight = container.ExpandableHeader?.Height ?? 0;

            float totalHeaderHeight = expandableHeaderHeight + fixedHeaderHeight;
            float effectiveHeaderHeight = totalHeaderHeight;

            // if we're in the "next page" of the sections container,
            // height of the expandable header should not be accounted.
            var scrollContent = container.ChildrenOfType<UserTrackingScrollContainer>().Single().ScrollContent;
            if (totalHeaderHeight + scrollContent.Height >= Content.DrawHeight)
                effectiveHeaderHeight -= expandableHeaderHeight;

            container.Add(new TestSection($"Section #{container.Children.Count + 1}")
            {
                Width = 300,
                Height = (Content.DrawHeight - effectiveHeaderHeight) * multiplier,
                Colour = default_colour
            });
        }

        private void triggerUserScroll(float direction)
        {
            InputManager.MoveMouseTo(container);
            InputManager.ScrollVerticalBy(direction);
        }

        private partial class TestDelayedLoadSection : TestSection
        {
            public TestDelayedLoadSection(string label)
                : base(label)
            {
                BackgroundColour = default_colour;
                Width = 300;
                AutoSizeAxes = Axes.Y;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Box box;

                Add(box = new Box
                {
                    Alpha = 0.01f,
                    RelativeSizeAxes = Axes.X,
                });

                // Emulate an operation that will be inhibited by IsMaskedAway.
                box.ResizeHeightTo(2000, 50);
            }
        }

        private partial class TestSection : TestBox
        {
            public bool Selected
            {
                set => BackgroundColour = value ? selected_colour : default_colour;
            }

            public TestSection(string label)
                : base(label)
            {
                BackgroundColour = default_colour;
            }
        }

        private partial class TestBox : Container
        {
            private readonly Box background;
            private readonly OsuSpriteText text;

            public ColourInfo BackgroundColour
            {
                set
                {
                    background.Colour = value;
                    text.Colour = OsuColour.ForegroundTextColourFor(value.AverageColour);
                }
            }

            public TestBox(string label)
            {
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    text = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = label,
                        Font = OsuFont.Default.With(size: 36),
                    }
                };
            }
        }
    }
}
