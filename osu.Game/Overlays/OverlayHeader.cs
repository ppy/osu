// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public abstract partial class OverlayHeader : Container
    {
        public OverlayTitle Title { get; }

        private float contentSidePadding;

        /// <summary>
        /// Horizontal padding of the header content.
        /// </summary>
        protected float ContentSidePadding
        {
            get => contentSidePadding;
            set
            {
                contentSidePadding = value;
                content.Padding = new MarginPadding
                {
                    Horizontal = value
                };
            }
        }

        private readonly Box titleBackground;
        private readonly Container content;

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
                                    content = new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Child = Title = CreateTitle().With(title =>
                                        {
                                            title.Anchor = Anchor.CentreLeft;
                                            title.Origin = Anchor.CentreLeft;
                                        }),
                                    }
                                }
                            },
                        }
                    },
                    CreateContent()
                }
            });

            ContentSidePadding = 50;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            titleBackground.Colour = colourProvider.Dark5;
        }

        [NotNull]
        protected virtual Drawable CreateContent() => Empty();

        [NotNull]
        protected virtual Drawable CreateBackground() => Empty();

        protected abstract OverlayTitle CreateTitle();
    }
}
