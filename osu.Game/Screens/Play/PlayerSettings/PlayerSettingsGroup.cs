// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Overlays;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public class PlayerSettingsGroup : SettingsToolboxGroup
    {
        public PlayerSettingsGroup(string title)
            : base(title)
        {
        }

        private ScheduledDelegate hoverExpandEvent;

        protected override bool OnHover(HoverEvent e)
        {
            updateHoverExpansion();
            base.OnHover(e);

            // Importantly, return true to correctly take focus away from PlayerLoader.
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (hoverExpandEvent != null)
            {
                hoverExpandEvent?.Cancel();
                hoverExpandEvent = null;

                Expanded.Value = false;
            }

            base.OnHoverLost(e);
        }

        private void updateHoverExpansion()
        {
            hoverExpandEvent?.Cancel();

            if (IsHovered && !Expanded.Value)
                hoverExpandEvent = Scheduler.AddDelayed(() => Expanded.Value = true, 0);
        }
    }
}
