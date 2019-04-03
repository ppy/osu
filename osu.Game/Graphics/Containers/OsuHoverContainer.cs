// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osuTK.Graphics;
using System.Collections.Generic;

namespace osu.Game.Graphics.Containers
{
    public class OsuHoverContainer : OsuClickableContainer
    {
        protected const float FADE_DURATION = 500;

        protected Color4 HoverColour;

        protected Color4 IdleColour = Color4.White;

        protected virtual IEnumerable<Drawable> EffectTargets => new[] { Content };

        protected override bool OnHover(HoverEvent e)
        {
            EffectTargets.ForEach(d => d.FadeColour(HoverColour, FADE_DURATION, Easing.OutQuint));
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            EffectTargets.ForEach(d => d.FadeColour(IdleColour, FADE_DURATION, Easing.OutQuint));
            base.OnHoverLost(e);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            HoverColour = colours.Yellow;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            EffectTargets.ForEach(d => d.FadeColour(IdleColour));
        }
    }
}
