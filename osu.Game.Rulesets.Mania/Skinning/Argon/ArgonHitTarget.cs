// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Argon
{
    public partial class ArgonHitTarget : CompositeDrawable
    {
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        [BackgroundDependencyLoader]
        private void load(IScrollingInfo scrollingInfo)
        {
            RelativeSizeAxes = Axes.X;
            Height = ArgonNotePiece.NOTE_HEIGHT * ArgonNotePiece.NOTE_ACCENT_RATIO;

            Masking = true;
            CornerRadius = ArgonNotePiece.CORNER_RADIUS;

            InternalChildren = new[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.3f,
                    Blending = BlendingParameters.Additive,
                    Colour = Color4.White
                },
            };

            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            Anchor = Origin = direction.NewValue == ScrollingDirection.Up ? Anchor.TopLeft : Anchor.BottomLeft;
        }
    }
}
