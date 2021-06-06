// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneColourPicker : OsuTestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create pickers", () => Child = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(),
                    new Dimension()
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = @"No OverlayColourProvider",
                                    Font = OsuFont.Default.With(size: 40)
                                },
                                new OsuColourPicker
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre
                                }
                            }
                        },
                        new ColourProvidingContainer(OverlayColourScheme.Blue)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = @"With blue OverlayColourProvider",
                                    Font = OsuFont.Default.With(size: 40)
                                },
                                new OsuColourPicker
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre
                                }
                            }
                        }
                    }
                }
            });
        }

        private class ColourProvidingContainer : Container
        {
            [Cached]
            private OverlayColourProvider provider { get; }

            public ColourProvidingContainer(OverlayColourScheme colourScheme)
            {
                provider = new OverlayColourProvider(colourScheme);
            }
        }
    }
}
