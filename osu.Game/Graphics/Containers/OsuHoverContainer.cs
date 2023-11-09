// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osuTK.Graphics;
using System.Collections.Generic;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Graphics.Containers
{
    public partial class OsuHoverContainer : OsuClickableContainer
    {
        protected const float FADE_DURATION = 500;

        protected Color4 HoverColour;

        public Color4 IdleColour = Color4.White;

        protected virtual IEnumerable<Drawable> EffectTargets => new[] { Content };

        public OsuHoverContainer(HoverSampleSet sampleSet = HoverSampleSet.Default)
            : base(sampleSet)
        {
            Enabled.ValueChanged += e =>
            {
                if (isHovered)
                {
                    if (e.NewValue)
                        fadeIn();
                    else
                        fadeOut();
                }
            };
        }

        private bool isHovered;

        protected override bool OnHover(HoverEvent e)
        {
            if (isHovered)
                return false;

            isHovered = true;

            if (!Enabled.Value)
                return false;

            fadeIn();

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (!isHovered)
                return;

            isHovered = false;
            fadeOut();

            base.OnHoverLost(e);
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

        private void fadeIn() => EffectTargets.ForEach(d => d.FadeColour(HoverColour, FADE_DURATION, Easing.OutQuint));

        private void fadeOut() => EffectTargets.ForEach(d => d.FadeColour(IdleColour, FADE_DURATION, Easing.OutQuint));
    }
}
