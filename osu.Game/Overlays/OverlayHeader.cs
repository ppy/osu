// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays
{
    public abstract class OverlayHeader : Container
    {
        protected readonly OverlayHeaderTabControl TabControl;

        private const float cover_height = 150;
        private const float cover_info_height = 75;

        protected OverlayHeader()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = cover_height,
                    Masking = true,
                    Child = CreateBackground()
                },
                new Container
                {
                    Margin = new MarginPadding { Left = UserProfileOverlay.CONTENT_X_MARGIN },
                    Y = cover_height,
                    Height = cover_info_height,
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.BottomLeft,
                    Depth = -float.MaxValue,
                    Children = new Drawable[]
                    {
                        CreateTitle().With(t => t.X = -ScreenTitle.ICON_WIDTH),
                        TabControl = new OverlayHeaderTabControl
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                            Height = cover_info_height - 30,
                            Margin = new MarginPadding { Left = -UserProfileOverlay.CONTENT_X_MARGIN },
                            Padding = new MarginPadding { Left = UserProfileOverlay.CONTENT_X_MARGIN }
                        }
                    }
                },
                new Container
                {
                    Margin = new MarginPadding { Top = cover_height },
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Child = CreateContent()
                }
            };
        }

        protected abstract Drawable CreateBackground();

        protected abstract Drawable CreateContent();

        protected abstract ScreenTitle CreateTitle();
    }
}
