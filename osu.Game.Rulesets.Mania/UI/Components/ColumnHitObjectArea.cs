// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.UI.Scrolling;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public class ColumnHitObjectArea : Container, IHasAccentColour
    {
        private const float hit_target_height = 10;
        private const float hit_target_bar_height = 2;

        private Container<Drawable> content;
        protected override Container<Drawable> Content => content;

        private Container hitTargetBar;

        [BackgroundDependencyLoader]
        private void load(ScrollingInfo scrollingInfo)
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Anchor = scrollingInfo.Direction == ScrollingDirection.Up ? Anchor.TopLeft : Anchor.BottomLeft,
                    Origin = scrollingInfo.Direction == ScrollingDirection.Up ? Anchor.TopLeft : Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = hit_target_height,
                    Colour = Color4.Black
                },
                hitTargetBar = new Container
                {
                    Anchor = scrollingInfo.Direction == ScrollingDirection.Up ? Anchor.TopLeft : Anchor.BottomLeft,
                    Origin = scrollingInfo.Direction == ScrollingDirection.Up ? Anchor.TopLeft : Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = hit_target_bar_height,
                    Masking = true,
                    Child = new Box { RelativeSizeAxes = Axes.Both }
                },
                content = new Container
                {
                    Name = "Hit objects",
                    RelativeSizeAxes = Axes.Both,
                },
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

            hitTargetBar.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Radius = 5,
                Colour = accentColour.Opacity(0.5f),
            };
        }
    }
}
