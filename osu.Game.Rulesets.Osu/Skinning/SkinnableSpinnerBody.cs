// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Skinning
{
    /// <summary>
    /// A skinnable drawable of the <see cref="OsuSkinComponents.SpinnerBody"/> component, with the approach circle exposed for modification.
    /// </summary>
    public class SkinnableSpinnerBody : SkinnableDrawable
    {
        private readonly Drawable approachCircleProxy;

        public SkinnableSpinnerBody(Drawable approachCircleProxy, Func<ISkinComponent, Drawable> defaultImplementation = null)
            : base(new OsuSkinComponent(OsuSkinComponents.SpinnerBody), defaultImplementation)
        {
            this.approachCircleProxy = approachCircleProxy;
        }

        protected override void SkinChanged(ISkinSource skin)
        {
            if (Drawable is IProxiesApproachCircle oldProxiesApproachCircle)
                oldProxiesApproachCircle.ApproachCircleTarget.Remove(approachCircleProxy);

            base.SkinChanged(skin);

            if (Drawable is IProxiesApproachCircle newProxiesApproachCircle)
                newProxiesApproachCircle.ApproachCircleTarget.Add(approachCircleProxy);
        }
    }
}
