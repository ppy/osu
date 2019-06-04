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

        public OsuHoverContainer()
        {
            Enabled.ValueChanged += e =>
            {
                if (!e.NewValue)
                    unhover();
            };
        }

        private bool isHovered;

        protected override bool OnHover(HoverEvent e)
        {
            if (!Enabled.Value)
                return false;

            EffectTargets.ForEach(d => d.FadeColour(HoverColour, FADE_DURATION, Easing.OutQuint));
            isHovered = true;

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            unhover();
            base.OnHoverLost(e);
        }

        private void unhover()
        {
            if (!isHovered)
                return;

            isHovered = false;
            EffectTargets.ForEach(d => d.FadeColour(IdleColour, FADE_DURATION, Easing.OutQuint));
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (HoverColour == default)
                HoverColour = colours.Yellow;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            EffectTargets.ForEach(d => d.FadeColour(IdleColour));
        }
    }
}
