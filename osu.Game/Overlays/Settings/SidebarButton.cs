// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings
{
    public abstract partial class SidebarButton : OsuButton
    {
        protected const double FADE_DURATION = 500;

        [Resolved]
        protected OverlayColourProvider ColourProvider { get; private set; } = null!;

        protected SidebarButton(HoverSampleSet? hoverSounds = HoverSampleSet.ButtonSidebar)
            : base(hoverSounds)
        {
        }

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

        protected abstract void UpdateState();
    }
}
