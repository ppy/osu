// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Mods
{
    public abstract partial class ModFooterInformationDisplay : CompositeDrawable
    {
        protected FillFlowContainer LeftContent { get; private set; } = null!;
        protected FillFlowContainer RightContent { get; private set; } = null!;
        protected Container Content { get; private set; } = null!;

        private Container innerContent = null!;

        protected Box MainBackground { get; private set; } = null!;
        protected Box FrontBackground { get; private set; } = null!;

        [Resolved]
        protected OverlayColourProvider ColourProvider { get; private set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = Content = new Container
            {
                Origin = Anchor.BottomRight,
                Anchor = Anchor.BottomRight,
                AutoSizeAxes = Axes.X,
                Height = ShearedButton.HEIGHT,
                Shear = new Vector2(ShearedOverlayContainer.SHEAR, 0),
                CornerRadius = ShearedButton.CORNER_RADIUS,
                BorderThickness = ShearedButton.BORDER_THICKNESS,
                Masking = true,
                Children = new Drawable[]
                {
                    MainBackground = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    new FillFlowContainer // divide inner and outer content
                    {
                        Origin = Anchor.BottomLeft,
                        Anchor = Anchor.BottomLeft,
                        AutoSizeAxes = Axes.X,
                        RelativeSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                        Children = new Drawable[]
                        {
                            innerContent = new Container
                            {
                                AutoSizeAxes = Axes.X,
                                RelativeSizeAxes = Axes.Y,
                                BorderThickness = ShearedButton.BORDER_THICKNESS,
                                CornerRadius = ShearedButton.CORNER_RADIUS,
                                Masking = true,
                                Children = new Drawable[]
                                {
                                    FrontBackground = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both
                                    },
                                    LeftContent = new FillFlowContainer // actual inner content
                                    {
                                        Origin = Anchor.Centre,
                                        Anchor = Anchor.Centre,
                                        RelativeSizeAxes = Axes.Y,
                                        AutoSizeAxes = Axes.X,
                                        Margin = new MarginPadding { Horizontal = 15 },
                                        Spacing = new Vector2(10),
                                    }
                                }
                            },
                            RightContent = new FillFlowContainer
                            {
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.X,
                                RelativeSizeAxes = Axes.Y,
                                Direction = FillDirection.Horizontal,
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            MainBackground.Colour = ColourProvider.Background4;
            FrontBackground.Colour = ColourProvider.Background3;
            Color4 glowColour = ColourProvider.Background1;

            Content.BorderColour = ColourInfo.GradientVertical(MainBackground.Colour, glowColour);
            innerContent.BorderColour = ColourInfo.GradientVertical(FrontBackground.Colour, glowColour);
        }
    }
}
