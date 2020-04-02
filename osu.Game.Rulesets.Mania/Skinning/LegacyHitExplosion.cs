// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning
{
    public class LegacyHitExplosion : LegacyManiaElement
    {
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private Drawable explosion;

        public LegacyHitExplosion()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, IScrollingInfo scrollingInfo)
        {
            string imageName = GetManiaSkinConfig<string>(skin, LegacyManiaSkinConfigurationLookups.ExplosionImage)?.Value
                               ?? "lightingN";

            InternalChild = explosion = skin.GetAnimation(imageName, true, false, startAtCurrentTime: true).With(d =>
            {
                if (d == null)
                    return;

                d.Origin = Anchor.Centre;
                d.Blending = BlendingParameters.Additive;

                if (!(d is TextureAnimation texAnimation))
                    return;

                if (texAnimation.FrameCount > 0)
                    texAnimation.DefaultFrameLength = Math.Max(texAnimation.DefaultFrameLength, 170.0 / texAnimation.FrameCount);
            });

            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            if (explosion != null)
                explosion.Anchor = direction.NewValue == ScrollingDirection.Up ? Anchor.TopCentre : Anchor.BottomCentre;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            explosion?.FadeInFromZero(80)
                     .Then().FadeOut(120);
        }
    }
}
