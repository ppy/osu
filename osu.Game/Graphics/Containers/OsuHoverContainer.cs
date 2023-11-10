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

        private Color4 idleColour = Color4.White;

        public Color4 IdleColour
        {
            get => idleColour;
            set
            {
                idleColour = value;
                updateColour();
            }
        }

        protected virtual IEnumerable<Drawable> EffectTargets => new[] { Content };

        public OsuHoverContainer(HoverSampleSet sampleSet = HoverSampleSet.Default)
            : base(sampleSet)
        {
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (!Enabled.Value)
                return false;

            updateColour();

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateColour();

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

            Enabled.BindValueChanged(_ => updateColour(), true);
            EffectTargets.ForEach(d => d.FinishTransforms());
        }

        private void updateColour()
        {
            EffectTargets.ForEach(d => d?.FadeColour(IsHovered && Enabled.Value ? HoverColour : IdleColour, FADE_DURATION, Easing.OutQuint));
        }
    }
}
