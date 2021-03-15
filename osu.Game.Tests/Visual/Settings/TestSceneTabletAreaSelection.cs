// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using OpenTabletDriver.Plugin.Tablet;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Settings
{
    [TestFixture]
    public class TestSceneTabletAreaSelection : OsuTestScene
    {
        private TabletAreaSelection areaSelection;

        [BackgroundDependencyLoader]
        private void load()
        {
            DigitizerIdentifier testTablet = new DigitizerIdentifier
            {
                // size specifications in millimetres.
                Width = 160,
                Height = 100,
            };

            AddRange(new[]
            {
                areaSelection = new TabletAreaSelection(testTablet)
                {
                    State = { Value = Visibility.Visible }
                }
            });
        }
    }

    public class TabletAreaSelection : OsuFocusedOverlayContainer
    {
        private readonly DigitizerIdentifier tablet;

        private readonly Container tabletContainer;
        private readonly Container usableAreaContainer;

        public TabletAreaSelection(DigitizerIdentifier tablet)
        {
            RelativeSizeAxes = Axes.Both;

            this.tablet = tablet;

            InternalChildren = new Drawable[]
            {
                tabletContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(3),
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White,
                        },
                        usableAreaContainer = new Container
                        {
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Yellow,
                                },
                                new OsuSpriteText
                                {
                                    Text = "usable area",
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Colour = Color4.Black,
                                    Font = OsuFont.Default.With(size: 12)
                                }
                            }
                        },
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // TODO: handle tablet device changes etc.
            tabletContainer.Size = new Vector2(tablet.Width, tablet.Height);

            usableAreaContainer.Position = new Vector2(10, 30);
            usableAreaContainer.Size = new Vector2(80, 60);
        }
    }
}
