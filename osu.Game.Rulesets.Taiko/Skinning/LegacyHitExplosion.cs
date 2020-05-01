// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Rulesets.Taiko.Skinning
{
    public class LegacyHitExplosion : CompositeDrawable
    {
        public LegacyHitExplosion(Drawable sprite)
        {
            InternalChild = sprite;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            AutoSizeAxes = Axes.Both;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            const double animation_time = 120;

            this.FadeInFromZero(animation_time).Then().FadeOut(animation_time * 1.5);

            this.ScaleTo(0.6f)
                .Then().ScaleTo(1.1f, animation_time * 0.8)
                .Then().ScaleTo(0.9f, animation_time * 0.4)
                .Then().ScaleTo(1f, animation_time * 0.2);

            Expire(true);
        }
    }
}
