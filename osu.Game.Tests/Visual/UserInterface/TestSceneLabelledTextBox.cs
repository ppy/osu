// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneLabelledTextBox : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(LabelledTextBox),
        };

        [TestCase(false)]
        [TestCase(true)]
        public void TestTextBox(bool hasDescription) => createTextBox(hasDescription);

        private void createTextBox(bool hasDescription = false)
        {
            AddStep("create component", () =>
            {
                LabelledTextBox component;

                Child = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 500,
                    AutoSizeAxes = Axes.Y,
                    Child = component = new LabelledTextBox
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Label = "Testing text",
                        PlaceholderText = "This is definitely working as intended",
                    }
                };

                component.Label = "a sample component";
                component.Description = hasDescription ? "this text describes the component" : string.Empty;
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                Padding = new MarginPadding { Left = 150, Right = 150 },
                Child = new LabelledTextBox
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Label = "Testing text",
                    PlaceholderText = "This is definitely working as intended",
                }
            };
        }
    }
}
