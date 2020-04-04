// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public abstract class OverlayHeader : Container
    {
        private readonly Box titleBackground;
        private readonly ScreenTitle title;

        protected readonly FillFlowContainer HeaderInfo;

        protected OverlayHeader()
        {
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
                            CreateBackground(),
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Children = new Drawable[]
                                {
                                    titleBackground = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Color4.Gray,
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Padding = new MarginPadding
                                        {
                                            Horizontal = UserProfileOverlay.CONTENT_X_MARGIN,
                                            Vertical = 10,
                                        },
                                        Children = new[]
                                        {
                                            title = CreateTitle().With(title =>
                                            {
                                                title.Anchor = Anchor.CentreLeft;
                                                title.Origin = Anchor.CentreLeft;
                                            }),
                                            CreateTitleContent().With(content =>
                                            {
                                                content.Anchor = Anchor.CentreRight;
                                                content.Origin = Anchor.CentreRight;
                                            })
                                        }
                                    }
                                }
                            },
                        }
                    },
                    CreateContent()
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            titleBackground.Colour = colourProvider.Dark5;
            title.AccentColour = colourProvider.Highlight1;
        }

        [NotNull]
        protected virtual Drawable CreateContent() => Empty();

        [NotNull]
        protected virtual Drawable CreateBackground() => Empty();

        /// <summary>
        /// Creates a <see cref="Drawable"/> on the opposite side of the <see cref="ScreenTitle"/>. Used mostly to create <see cref="OverlayRulesetSelector"/>.
        /// </summary>
        [NotNull]
        protected virtual Drawable CreateTitleContent() => Empty();

        protected abstract ScreenTitle CreateTitle();
    }
}
