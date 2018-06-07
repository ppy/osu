// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;

namespace osu.Game.Rulesets.Mania.UI
{
    internal class HitExplosion : CompositeDrawable
    {
        private readonly CircularContainer circle;

        public HitExplosion(DrawableHitObject judgedObject)
        {
            bool isTick = judgedObject is DrawableHoldNoteTick;

            Anchor = Anchor.TopCentre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.X;
            Y = NotePiece.NOTE_HEIGHT / 2;
            Height = NotePiece.NOTE_HEIGHT;

            // scale roughly in-line with visual appearance of notes
            Scale = new Vector2(isTick ? 0.4f : 0.8f);

            InternalChild = circle = new CircularContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                // we want our size to be very small so the glow dominates it.
                Size = new Vector2(0.1f),
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Colour = Interpolation.ValueAt(0.1f, judgedObject.AccentColour, Color4.White, 0, 1),
                    Radius = 100,
                },
                Child = new Box
                {
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both,
                    AlwaysPresent = true
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            circle.ResizeTo(circle.Size * new Vector2(4, 20), 1000, Easing.OutQuint);
            this.FadeIn(16).Then().FadeOut(500, Easing.OutQuint);

            Expire(true);
        }
    }
}
