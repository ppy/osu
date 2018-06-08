// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Game.Graphics;
using osu.Game.Rulesets.UI.Scrolling;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public class ColumnBackground : CompositeDrawable, IKeyBindingHandler<ManiaAction>, IHasAccentColour
    {
        public ManiaAction Action;

        private Box background;
        private Box backgroundOverlay;

        private ScrollingInfo scrollingInfo;

        [BackgroundDependencyLoader]
        private void load(ScrollingInfo scrollingInfo)
        {
            this.scrollingInfo = scrollingInfo;

            InternalChildren = new[]
            {
                background = new Box
                {
                    Name = "Background",
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.3f
                },
                backgroundOverlay = new Box
                {
                    Name = "Background Gradient Overlay",
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                    Anchor = scrollingInfo.Direction == ScrollingDirection.Up ? Anchor.TopLeft : Anchor.BottomLeft,
                    Origin = scrollingInfo.Direction == ScrollingDirection.Up ? Anchor.TopLeft : Anchor.BottomLeft,
                    Blending = BlendingMode.Additive,
                    Alpha = 0
                }
            };
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

            background.Colour = AccentColour;

            var brightPoint = AccentColour.Opacity(0.6f);
            var dimPoint = AccentColour.Opacity(0);

            backgroundOverlay.Colour = ColourInfo.GradientVertical(
                scrollingInfo.Direction == ScrollingDirection.Up ? brightPoint : dimPoint,
                scrollingInfo.Direction == ScrollingDirection.Up ? dimPoint : brightPoint);
        }

        public bool OnPressed(ManiaAction action)
        {
            if (action == Action)
                backgroundOverlay.FadeTo(1, 50, Easing.OutQuint).Then().FadeTo(0.5f, 250, Easing.OutQuint);
            return false;
        }

        public bool OnReleased(ManiaAction action)
        {
            if (action == Action)
                backgroundOverlay.FadeTo(0, 250, Easing.OutQuint);
            return false;
        }
    }
}
