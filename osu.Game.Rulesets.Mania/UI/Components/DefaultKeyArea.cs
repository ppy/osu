// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public partial class DefaultKeyArea : CompositeDrawable, IKeyBindingHandler<ManiaAction>
    {
        private const float key_icon_size = 10;
        private const float key_icon_corner_radius = 3;

        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private Container directionContainer;
        private Container keyIcon;
        private Drawable gradient;

        private Bindable<Color4> accentColour;

        [Resolved]
        private Column column { get; set; }

        public DefaultKeyArea()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(IScrollingInfo scrollingInfo)
        {
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
                    keyIcon = new Container
                    {
                        Name = "Key icon",
                        Size = new Vector2(key_icon_size),
                        Origin = Anchor.Centre,
                        Masking = true,
                        CornerRadius = key_icon_corner_radius,
                        BorderThickness = 2,
                        BorderColour = Color4.White, // Not true
                        Children = new[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                AlwaysPresent = true
                            }
                        }
                    },
                }
            };

            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);

            accentColour = column.AccentColour.GetBoundCopy();
            accentColour.BindValueChanged(colour =>
            {
                keyIcon.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Radius = 5,
                    Colour = colour.NewValue.Opacity(0.5f),
                };
            }, true);
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            if (direction.NewValue == ScrollingDirection.Up)
            {
                keyIcon.Anchor = Anchor.BottomCentre;
                keyIcon.Y = -20;
                directionContainer.Anchor = directionContainer.Origin = Anchor.TopLeft;
                gradient.Colour = ColourInfo.GradientVertical(Color4.Black, Color4.Black.Opacity(0));
            }
            else
            {
                keyIcon.Anchor = Anchor.TopCentre;
                keyIcon.Y = 20;
                directionContainer.Anchor = directionContainer.Origin = Anchor.BottomLeft;
                gradient.Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0), Color4.Black);
            }
        }

        public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            if (e.Action == column.Action.Value)
                keyIcon.ScaleTo(1.4f, 50, Easing.OutQuint).Then().ScaleTo(1.3f, 250, Easing.OutQuint);
            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e)
        {
            if (e.Action == column.Action.Value)
                keyIcon.ScaleTo(1f, 125, Easing.OutQuint);
        }
    }
}
