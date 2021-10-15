// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneSettingsCheckbox : OsuTestScene
    {
        [TestCase]
        public void TestCheckbox()
        {
            AddStep("create component", () =>
            {
                FillFlowContainer flow;

                Child = flow = new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 500,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(5),
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new SettingsCheckbox
                        {
                            LabelText = "a sample component",
                        },
                    },
                };

                foreach (var colour1 in Enum.GetValues(typeof(OverlayColourScheme)).OfType<OverlayColourScheme>())
                {
                    flow.Add(new OverlayColourContainer(colour1)
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Child = new SettingsCheckbox
                        {
                            LabelText = "a sample component",
                        }
                    });
                }
            });
        }

        private class OverlayColourContainer : Container
        {
            [Cached]
            private OverlayColourProvider colourProvider;

            public OverlayColourContainer(OverlayColourScheme scheme)
            {
                colourProvider = new OverlayColourProvider(scheme);
            }
        }
    }
}
