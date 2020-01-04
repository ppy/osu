// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Game.Graphics;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public class ColumnBackground : CompositeDrawable, IKeyBindingHandler<ManiaAction>, IHasAccentColour
    {
        private readonly IBindable<ManiaAction> action = new Bindable<ManiaAction>();

        private Box background;
        private Box backgroundOverlay;

        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        [BackgroundDependencyLoader]
        private void load(IBindable<ManiaAction> action, IScrollingInfo scrollingInfo)
        {
            this.action.BindTo(action);

            InternalChildren = new[]
            {
                background = new Box
                {
                    Name = "Background",
                    RelativeSizeAxes = Axes.Both,
                },
                backgroundOverlay = new Box
                {
                    Name = "Background Gradient Overlay",
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0
                }
            };

            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(dir =>
            {
                backgroundOverlay.Anchor = backgroundOverlay.Origin = dir.NewValue == ScrollingDirection.Up ? Anchor.TopLeft : Anchor.BottomLeft;
                updateColours();
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

            background.Colour = AccentColour.Darken(5);

            var brightPoint = AccentColour.Opacity(0.6f);
            var dimPoint = AccentColour.Opacity(0);

            backgroundOverlay.Colour = ColourInfo.GradientVertical(
                direction.Value == ScrollingDirection.Up ? brightPoint : dimPoint,
                direction.Value == ScrollingDirection.Up ? dimPoint : brightPoint);
        }

        public bool OnPressed(ManiaAction action)
        {
            if (action == this.action.Value)
                backgroundOverlay.FadeTo(1, 50, Easing.OutQuint).Then().FadeTo(0.5f, 250, Easing.OutQuint);
            return false;
        }

        public bool OnReleased(ManiaAction action)
        {
            if (action == this.action.Value)
                backgroundOverlay.FadeTo(0, 250, Easing.OutQuint);
            return false;
        }
    }
}
