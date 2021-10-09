// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings
{
    public abstract class SidebarButton : OsuButton
    {
        private const double fade_duration = 50;

        [Resolved]
        protected OverlayColourProvider ColourProvider { get; private set; }

        protected abstract Drawable HoverTarget { get; }

        [BackgroundDependencyLoader]
        private void load()
        {
            BackgroundColour = ColourProvider.Background5;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            UpdateState();
            FinishTransforms(true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            UpdateState();
            return false;
        }

        protected override void OnHoverLost(HoverLostEvent e) => UpdateState();

        protected virtual void UpdateState()
        {
            HoverTarget.FadeColour(IsHovered ? ColourProvider.Light1 : ColourProvider.Light3, fade_duration, Easing.OutQuint);
        }
    }
}
