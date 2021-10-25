// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
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
        [Resolved(canBeNull: true)]
        private DrawableHitObject drawableHitObject { get; set; }

        private Drawable proxy;

        [BackgroundDependencyLoader]
        private void load(ISkinSource skinSource)
        {
            AutoSizeAxes = Axes.Both;

            string lookupName = new OsuSkinComponent(OsuSkinComponents.ReverseArrow).LookupName;

            var skin = skinSource.FindProvider(s => s.GetTexture(lookupName) != null);
            InternalChild = skin?.GetAnimation(lookupName, true, true) ?? Empty();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            proxy = CreateProxy();

            if (drawableHitObject != null)
            {
                drawableHitObject.HitObjectApplied += onHitObjectApplied;
                onHitObjectApplied(drawableHitObject);
            }
        }

        private void onHitObjectApplied(DrawableHitObject drawableObject)
        {
            Debug.Assert(proxy.Parent == null);

            // see logic in LegacySliderHeadHitCircle.
            (drawableObject as DrawableSliderRepeat)?.DrawableSlider
                                                    .OverlayElementContainer.Add(proxy);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            if (drawableHitObject != null)
                drawableHitObject.HitObjectApplied -= onHitObjectApplied;
        }
    }
}
