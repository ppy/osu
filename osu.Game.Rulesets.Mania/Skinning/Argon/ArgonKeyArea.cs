// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Argon
{
    public class ArgonKeyArea : CompositeDrawable, IKeyBindingHandler<ManiaAction>
    {
        private const float key_icon_size = 10;

        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private Container directionContainer = null!;
        private Container<Circle> keyIcon = null!;
        private Drawable gradient = null!;

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
            const float icon_spacing = 8;
            const float icon_vertical_offset = 20;

            InternalChild = directionContainer = new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = Stage.HIT_TARGET_POSITION,
                Children = new[]
                {
                    gradient = new Box
                    {
                        Name = "Key gradient",
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.5f
                    },
                    keyIcon = new Container<Circle>
                    {
                        Name = "Icons",
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Children = new[]
                        {
                            new Circle
                            {
                                Y = icon_vertical_offset,
                                Size = new Vector2(icon_circle_size),
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Blending = BlendingParameters.Additive,
                                Colour = column.AccentColour,
                                Masking = true,
                            },
                            new Circle
                            {
                                X = -icon_spacing,
                                Y = icon_vertical_offset + icon_spacing * 1.2f,
                                Size = new Vector2(icon_circle_size),
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Blending = BlendingParameters.Additive,
                                Colour = column.AccentColour,
                                Masking = true,
                            },
                            new Circle
                            {
                                X = icon_spacing,
                                Y = icon_vertical_offset + icon_spacing * 1.2f,
                                Size = new Vector2(icon_circle_size),
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Blending = BlendingParameters.Additive,
                                Colour = column.AccentColour,
                                Masking = true,
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
                    gradient.Colour = ColourInfo.GradientVertical(Color4.Black, Color4.Black.Opacity(0));
                    break;

                case ScrollingDirection.Down:
                    directionContainer.Scale = new Vector2(1, 1);
                    directionContainer.Anchor = Anchor.BottomLeft;
                    directionContainer.Origin = Anchor.BottomLeft;
                    gradient.Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0), Color4.Black);
                    break;
            }
        }

        public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            if (e.Action == column.Action.Value)
            {
                foreach (var circle in keyIcon.Children)
                {
                    circle.ScaleTo(1.1f, 50, Easing.OutQuint);

                    circle.FadeColour(Color4.White, 50, Easing.OutQuint);
                    circle.TransformTo(nameof(EdgeEffect), new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = Color4.White.Opacity(0.05f),
                        Radius = 10,
                    }, 50, Easing.OutQuint);
                }
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e)
        {
            if (e.Action == column.Action.Value)
            {
                foreach (var circle in keyIcon.Children)
                {
                    circle.ScaleTo(1f, 125, Easing.OutQuint);

                    circle.FadeColour(column.AccentColour, 200, Easing.OutQuint);
                    circle.TransformTo(nameof(EdgeEffect), new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = Color4.White.Opacity(0),
                        Radius = 10,
                    }, 200, Easing.OutQuint);
                }
            }
        }
    }
}
