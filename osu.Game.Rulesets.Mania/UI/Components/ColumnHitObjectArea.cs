// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public class ColumnHitObjectArea : CompositeDrawable, IHasAccentColour
    {
        private const float hit_target_bar_height = 2;

        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private readonly Container hitTargetLine;
        private readonly Drawable hitTargetBar;

        public ColumnHitObjectArea(HitObjectContainer hitObjectContainer)
        {
            InternalChildren = new[]
            {
                hitTargetBar = new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = NotePiece.NOTE_HEIGHT,
                    Alpha = 0.6f,
                    Colour = Color4.Black
                },
                hitTargetLine = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = hit_target_bar_height,
                    Masking = true,
                    Child = new Box { RelativeSizeAxes = Axes.Both }
                },
                hitObjectContainer
            };
        }

        [BackgroundDependencyLoader]
        private void load(IScrollingInfo scrollingInfo)
        {
            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(dir =>
            {
                Anchor anchor = dir.NewValue == ScrollingDirection.Up ? Anchor.TopLeft : Anchor.BottomLeft;

                hitTargetBar.Anchor = hitTargetBar.Origin = anchor;
                hitTargetLine.Anchor = hitTargetLine.Origin = anchor;
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

            hitTargetLine.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Radius = 5,
                Colour = accentColour.Opacity(0.5f),
            };
        }
    }
}
