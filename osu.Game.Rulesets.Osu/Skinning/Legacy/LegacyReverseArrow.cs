// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public class LegacyReverseArrow : CompositeDrawable
    {
        private ISkin skin { get; set; }

        [Resolved(canBeNull: true)]
        private DrawableHitObject drawableHitObject { get; set; }

        public LegacyReverseArrow(ISkin skin)
        {
            this.skin = skin;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            string lookupName = new OsuSkinComponent(OsuSkinComponents.ReverseArrow).LookupName;

            InternalChild = skin.GetAnimation(lookupName, true, true) ?? Drawable.Empty();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // see logic in LegacySliderHeadHitCircle.
            (drawableHitObject as DrawableSliderRepeat)?.DrawableSlider
                                                       .OverlayElementContainer.Add(CreateProxy());
        }
    }
}
