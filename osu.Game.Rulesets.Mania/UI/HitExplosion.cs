// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.UI
{
    internal class HitExplosion : CompositeDrawable
    {
        private readonly Box inner;

        public HitExplosion(DrawableHitObject judgedObject)
        {
            bool isTick = judgedObject is DrawableHoldNoteTick;

            Anchor = Anchor.TopCentre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(isTick ? 0.5f : 1);
            FillMode = FillMode.Fit;

            Blending = BlendingMode.Additive;

            Color4 accent = isTick ? Color4.White : judgedObject.AccentColour;

            InternalChild = new CircularContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                BorderThickness = 1,
                BorderColour = accent,
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Colour = accent,
                    Radius = 10,
                    Hollow = true
                },
                Child = inner = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = accent,
                    Alpha = 1,
                    AlwaysPresent = true,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.ScaleTo(2f, 600, Easing.OutQuint).FadeOut(500);
            inner.FadeOut(250);

            Expire(true);
        }
    }
}
