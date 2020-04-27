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

            this.FadeIn(120);
            this.ScaleTo(0.6f).Then().ScaleTo(1, 240, Easing.OutElastic);
        }
    }
}
