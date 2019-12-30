// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public abstract class OverlayHeader : Container
    {
        private readonly Container background;
        private readonly Box titleBackground;
        private readonly ScreenTitle title;

        protected readonly FillFlowContainer HeaderInfo;
        protected readonly OverlayColourScheme ColourScheme;

        protected float BackgroundHeight
        {
            set => background.Height = value;
        }

        protected OverlayHeader(OverlayColourScheme colourScheme)
        {
            ColourScheme = colourScheme;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Add(new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new[]
                {
                    HeaderInfo = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Depth = -float.MaxValue,
                        Children = new[]
                        {
                            background = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                Masking = true,
                                Height = 80,
                                Child = CreateBackground()
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Children = new[]
                                {
                                    titleBackground = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        
                                    },
                                    title = CreateTitle().With(title =>
                                    {
                                        title.Anchor = Anchor.CentreLeft;
                                        title.Origin = Anchor.CentreLeft;
                                        title.Margin = new MarginPadding
                                        {
                                            Vertical = 10,
                                            Left = UserProfileOverlay.CONTENT_X_MARGIN
                                        };
                                    }),
                                    CreateTitleContent().With(content =>
                                    {
                                        content.Anchor = Anchor.CentreRight;
                                        content.Origin = Anchor.CentreRight;
                                        content.Margin = new MarginPadding
                                        {
                                            Vertical = 10,
                                            Right = UserProfileOverlay.CONTENT_X_MARGIN
                                        };
                                    })
                                }
                            }
                        }
                    },
                    CreateContent()
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            titleBackground.Colour = Color4.FromHsl(new Vector4(colours.GetBaseHue(ColourScheme), 0.2f, 0.15f, 1));
            title.AccentColour = Color4.FromHsl(new Vector4(colours.GetBaseHue(ColourScheme), 1, 0.7f, 1));
        }

        [NotNull]
        protected virtual Drawable CreateBackground() => new Container();

        [NotNull]
        protected virtual Drawable CreateContent() => new Container();

        [NotNull]
        protected virtual Drawable CreateTitleContent() => new Container();

        protected abstract ScreenTitle CreateTitle();
    }
}
