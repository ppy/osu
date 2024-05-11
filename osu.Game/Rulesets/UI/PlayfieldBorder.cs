// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Game.Extensions;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// Provides a border around the playfield.
    /// </summary>
    public partial class PlayfieldBorder : CompositeDrawable
    {
        public Bindable<PlayfieldBorderStyle> PlayfieldBorderStyle { get; } = new Bindable<PlayfieldBorderStyle>();

        private const int fade_duration = 500;

        private const float corner_length = 0.05f;
        private const float corner_thickness = 2;

        public PlayfieldBorder()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new Line(Direction.Horizontal)
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                },
                new Line(Direction.Horizontal)
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                },
                new Line(Direction.Horizontal)
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                },
                new Line(Direction.Horizontal)
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                },
                new Line(Direction.Vertical)
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                },
                new Line(Direction.Vertical)
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                },
                new Line(Direction.Vertical)
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                },
                new Line(Direction.Vertical)
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            this.ApplyGameWideClock(host);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            PlayfieldBorderStyle.BindValueChanged(updateStyle, true);
        }

        private void updateStyle(ValueChangedEvent<PlayfieldBorderStyle> style)
        {
            switch (style.NewValue)
            {
                case UI.PlayfieldBorderStyle.None:
                    this.FadeOut(fade_duration, Easing.OutQuint);
                    foreach (var line in InternalChildren.OfType<Line>())
                        line.TweenLength(0);

                    break;

                case UI.PlayfieldBorderStyle.Corners:
                    this.FadeIn(fade_duration, Easing.OutQuint);
                    foreach (var line in InternalChildren.OfType<Line>())
                        line.TweenLength(corner_length);

                    break;

                case UI.PlayfieldBorderStyle.Full:
                    this.FadeIn(fade_duration, Easing.OutQuint);
                    foreach (var line in InternalChildren.OfType<Line>())
                        line.TweenLength(0.5f);

                    break;
            }
        }

        private partial class Line : Box
        {
            private readonly Direction direction;

            public Line(Direction direction)
            {
                this.direction = direction;

                Colour = Color4.White;
                // starting in relative avoids the framework thinking it knows best and setting the width to 1 initially.

                switch (direction)
                {
                    case Direction.Horizontal:
                        RelativeSizeAxes = Axes.X;
                        Size = new Vector2(0, corner_thickness);
                        break;

                    case Direction.Vertical:
                        RelativeSizeAxes = Axes.Y;
                        Size = new Vector2(corner_thickness, 0);
                        break;
                }
            }

            public void TweenLength(float value)
            {
                switch (direction)
                {
                    case Direction.Horizontal:
                        this.ResizeWidthTo(value, fade_duration, Easing.OutQuint);
                        break;

                    case Direction.Vertical:
                        this.ResizeHeightTo(value, fade_duration, Easing.OutQuint);
                        break;
                }
            }
        }
    }
}
