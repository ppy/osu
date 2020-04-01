// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.Skinning
{
    public class LegacyHitExplosion : LegacyManiaColumnElement
    {
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private Drawable explosion;

        [BackgroundDependencyLoader]
        private void load(IScrollingInfo scrollingInfo)
        {
            InternalChild = explosion = new Sprite
            {
            };

            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);

            // Todo: LightingN
            // Todo: LightingL
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> obj)
        {
            throw new System.NotImplementedException();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            lighting.FadeInFromZero(80)
                    .Then().FadeOut(120);
        }
    }
}
