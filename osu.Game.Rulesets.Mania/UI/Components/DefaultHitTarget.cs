// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Mania.Skinning.Default;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public class DefaultHitTarget : CompositeDrawable
    {
        private const float hit_target_bar_height = 2;

        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private Container hitTargetLine;
        private Drawable hitTargetBar;

        [Resolved]
        private Column column { get; set; }

        public DefaultHitTarget()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(IScrollingInfo scrollingInfo)
        {
            InternalChildren = new[]
            {
                hitTargetBar = new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = DefaultNotePiece.NOTE_HEIGHT,
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
            };

            hitTargetLine.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Radius = 5,
                Colour = column.AccentColour.Opacity(0.5f),
            };

            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            if (direction.NewValue == ScrollingDirection.Up)
            {
                hitTargetBar.Anchor = hitTargetBar.Origin = Anchor.TopLeft;
                hitTargetLine.Anchor = hitTargetLine.Origin = Anchor.TopLeft;
            }
            else
            {
                hitTargetBar.Anchor = hitTargetBar.Origin = Anchor.BottomLeft;
                hitTargetLine.Anchor = hitTargetLine.Origin = Anchor.BottomLeft;
            }
        }
    }
}
