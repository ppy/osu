// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Argon
{
    public partial class ArgonHitExplosion : CompositeDrawable, IHitExplosion
    {
        public override bool RemoveWhenNotAlive => true;

        [Resolved]
        private Column column { get; set; } = null!;

        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private Container largeFaint = null!;

        private Bindable<Color4> accentColour = null!;

        public ArgonHitExplosion()
        {
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.X;
            Height = ArgonNotePiece.NOTE_HEIGHT;
        }

        [BackgroundDependencyLoader]
        private void load(IScrollingInfo scrollingInfo)
        {
            InternalChildren = new Drawable[]
            {
                largeFaint = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Height = ArgonNotePiece.NOTE_ACCENT_RATIO,
                    Masking = true,
                    CornerRadius = ArgonNotePiece.CORNER_RADIUS,
                    Blending = BlendingParameters.Additive,
                    Child = new Box
                    {
                        Colour = Color4.White,
                        RelativeSizeAxes = Axes.Both,
                    },
                },
            };

            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);

            accentColour = column.AccentColour.GetBoundCopy();
            accentColour.BindValueChanged(colour =>
            {
                largeFaint.Colour = Interpolation.ValueAt(0.8f, colour.NewValue, Color4.White, 0, 1);

                largeFaint.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Colour = colour.NewValue,
                    Roundness = 40,
                    Radius = 60,
                };
            }, true);
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            if (direction.NewValue == ScrollingDirection.Up)
            {
                Anchor = Anchor.TopCentre;
                largeFaint.Anchor = Anchor.TopCentre;
                largeFaint.Origin = Anchor.TopCentre;
                Y = ArgonNotePiece.NOTE_HEIGHT / 2;
            }
            else
            {
                Anchor = Anchor.BottomCentre;
                largeFaint.Anchor = Anchor.BottomCentre;
                largeFaint.Origin = Anchor.BottomCentre;
                Y = -ArgonNotePiece.NOTE_HEIGHT / 2;
            }
        }

        public void Animate(JudgementResult result)
        {
            this.FadeOutFromOne(PoolableHitExplosion.DURATION, Easing.Out);
        }
    }
}
