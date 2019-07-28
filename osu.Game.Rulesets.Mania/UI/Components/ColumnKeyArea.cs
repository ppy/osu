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
using osu.Game.Graphics;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public class ColumnKeyArea : CompositeDrawable, IKeyBindingHandler<ManiaAction>, IHasAccentColour
    {
        private const float key_icon_size = 10;
        private const float key_icon_corner_radius = 3;

        private readonly IBindable<ManiaAction> action = new Bindable<ManiaAction>();
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private Container keyIcon;

        [BackgroundDependencyLoader]
        private void load(IBindable<ManiaAction> action, IScrollingInfo scrollingInfo)
        {
            this.action.BindTo(action);

            Drawable gradient;

            InternalChildren = new[]
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
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(key_icon_size),
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
                }
            };

            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(dir =>
            {
                gradient.Colour = ColourInfo.GradientVertical(
                    dir.NewValue == ScrollingDirection.Up ? Color4.Black : Color4.Black.Opacity(0),
                    dir.NewValue == ScrollingDirection.Up ? Color4.Black.Opacity(0) : Color4.Black);
            }, true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateColours();
        }

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                if (accentColour == value)
                    return;

                accentColour = value;

                updateColours();
            }
        }

        private void updateColours()
        {
            if (!IsLoaded)
                return;

            keyIcon.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Radius = 5,
                Colour = accentColour.Opacity(0.5f),
            };
        }

        public bool OnPressed(ManiaAction action)
        {
            if (action == this.action.Value)
                keyIcon.ScaleTo(1.4f, 50, Easing.OutQuint).Then().ScaleTo(1.3f, 250, Easing.OutQuint);
            return false;
        }

        public bool OnReleased(ManiaAction action)
        {
            if (action == this.action.Value)
                keyIcon.ScaleTo(1f, 125, Easing.OutQuint);
            return false;
        }
    }
}
