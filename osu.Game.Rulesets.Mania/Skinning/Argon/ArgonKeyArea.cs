// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Argon
{
    public class ArgonKeyArea : CompositeDrawable, IKeyBindingHandler<ManiaAction>
    {
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private Container directionContainer = null!;
        private Container keyIcon = null!;
        private Drawable background = null!;

        private Circle hitTargetLine = null!;

        [Resolved]
        private Column column { get; set; } = null!;

        public ArgonKeyArea()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(IScrollingInfo scrollingInfo)
        {
            const float icon_circle_size = 8;
            const float icon_spacing = 7;
            const float icon_vertical_offset = -30;

            InternalChild = directionContainer = new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = Stage.HIT_TARGET_POSITION,
                Children = new[]
                {
                    background = new Box
                    {
                        Name = "Key gradient",
                        RelativeSizeAxes = Axes.Both,
                        Colour = column.AccentColour.Darken(0.6f),
                    },
                    keyIcon = new Container
                    {
                        Name = "Icons",
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Children = new[]
                        {
                            hitTargetLine = new Circle()
                            {
                                RelativeSizeAxes = Axes.X,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.Centre,
                                Colour = OsuColour.Gray(196 / 255f),
                                Height = 4,
                                Masking = true,
                            },
                            new Circle
                            {
                                Y = icon_vertical_offset,
                                Size = new Vector2(icon_circle_size),
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.Centre,
                                Blending = BlendingParameters.Additive,
                                Colour = column.AccentColour,
                                Masking = true,
                            },
                            new Circle
                            {
                                X = -icon_spacing,
                                Y = icon_vertical_offset + icon_spacing * 1.2f,
                                Size = new Vector2(icon_circle_size),
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.Centre,
                                Blending = BlendingParameters.Additive,
                                Colour = column.AccentColour,
                                Masking = true,
                            },
                            new Circle
                            {
                                X = icon_spacing,
                                Y = icon_vertical_offset + icon_spacing * 1.2f,
                                Size = new Vector2(icon_circle_size),
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.Centre,
                                Blending = BlendingParameters.Additive,
                                Colour = column.AccentColour,
                                Masking = true,
                            },
                            new CircularContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.Centre,
                                Y = -icon_vertical_offset,
                                Size = new Vector2(22, 14),
                                Masking = true,
                                BorderThickness = 4,
                                BorderColour = Color4.White,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Alpha = 0,
                                        AlwaysPresent = true,
                                    },
                                },
                            }
                        }
                    },
                }
            };

            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            switch (direction.NewValue)
            {
                case ScrollingDirection.Up:
                    directionContainer.Scale = new Vector2(1, -1);
                    directionContainer.Anchor = Anchor.TopLeft;
                    directionContainer.Origin = Anchor.BottomLeft;
                    break;

                case ScrollingDirection.Down:
                    directionContainer.Scale = new Vector2(1, 1);
                    directionContainer.Anchor = Anchor.BottomLeft;
                    directionContainer.Origin = Anchor.BottomLeft;
                    break;
            }
        }

        public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            if (e.Action == column.Action.Value)
            {
                background
                    .FadeColour(column.AccentColour.Lighten(0.3f), 50, Easing.OutQuint).Then()
                    .FadeColour(column.AccentColour, 100, Easing.OutQuint);

                foreach (var circle in keyIcon.Children.OfType<CompositeDrawable>())
                {
                    if (circle != hitTargetLine)
                        circle.ScaleTo(0.9f, 50, Easing.OutQuint);

                    circle.FadeColour(Color4.White, 50, Easing.OutQuint);

                    // TODO: VERY TMPOERAOIRY.
                    float f = circle == hitTargetLine ? 0.2f : (circle is Circle ? 0.05f : 0.2f);

                    circle.TransformTo(nameof(EdgeEffect), new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = Color4.White.Opacity(f),
                        Radius = 40,
                    }, 50, Easing.OutQuint);
                }
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e)
        {
            if (e.Action == column.Action.Value)
            {
                background.FadeColour(column.AccentColour.Darken(0.6f), 800, Easing.OutQuint);

                foreach (var circle in keyIcon.Children.OfType<CompositeDrawable>())
                {
                    circle.ScaleTo(1f, 200, Easing.OutQuint);

                    // TODO: temp lol
                    if (circle == hitTargetLine)
                    {
                        circle.FadeColour(OsuColour.Gray(196 / 255f), 800, Easing.OutQuint);
                    }
                    else if (circle is Circle)
                        circle.FadeColour(column.AccentColour, 800, Easing.OutQuint);

                    circle.TransformTo(nameof(EdgeEffect), new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = Color4.White.Opacity(0),
                        Radius = 30,
                    }, 800, Easing.OutQuint);
                }
            }
        }
    }
}
