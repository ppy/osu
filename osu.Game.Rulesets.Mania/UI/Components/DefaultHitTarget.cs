// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
    public partial class DefaultHitTarget : CompositeDrawable
    {
        private const float hit_target_bar_height = 2;

        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private Container hitTargetLine;
        private Drawable hitTargetBar;

        private Bindable<Color4> accentColour;

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

            accentColour = column.AccentColour.GetBoundCopy();
            accentColour.BindValueChanged(colour =>
            {
                hitTargetLine.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Radius = 5,
                    Colour = colour.NewValue.Opacity(0.5f),
                };
            }, true);

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
