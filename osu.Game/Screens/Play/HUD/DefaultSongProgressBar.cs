// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;

namespace osu.Game.Screens.Play.HUD
{
    public partial class DefaultSongProgressBar : SongProgressBar
    {
        public Color4 FillColour
        {
            set => fill.Colour = value;
        }

        private readonly Box fill;
        private readonly Container handleBase;
        private readonly Container handleContainer;

        public DefaultSongProgressBar(float barHeight, float handleBarHeight, Vector2 handleSize)
        {
            RelativeSizeAxes = Axes.X;
            Height = barHeight + handleBarHeight + handleSize.Y;

            Children = new Drawable[]
            {
                new Box
                {
                    Name = "Background",
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = barHeight,
                    Colour = Color4.Black,
                    Alpha = 0.5f,
                    Depth = 1,
                },
                fill = new Box
                {
                    Name = "Fill",
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Height = barHeight,
                },
                handleBase = new Container
                {
                    Name = "HandleBar container",
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    Width = 2,
                    Alpha = 0,
                    Colour = Color4.White,
                    Position = new Vector2(2, 0),
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Name = "HandleBar box",
                            RelativeSizeAxes = Axes.Both,
                        },
                        handleContainer = new Container
                        {
                            Name = "Handle container",
                            Origin = Anchor.BottomCentre,
                            Anchor = Anchor.TopCentre,
                            Size = handleSize,
                            CornerRadius = 5,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Name = "Handle box",
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.White
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            InteractiveBindable.BindValueChanged(i => handleBase.FadeTo(i.NewValue ? 1 : 0, 200), true);
        }

        protected override void Update()
        {
            base.Update();

            handleBase.Height = Height - handleContainer.Height;
            float newX = (float)Interpolation.Lerp(handleBase.X, NormalizedValue * DrawWidth, Math.Clamp(Time.Elapsed / 40, 0, 1));

            fill.Width = newX;
            handleBase.X = newX;
        }
    }
}
