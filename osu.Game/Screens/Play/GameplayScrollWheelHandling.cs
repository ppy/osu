// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Overlays.Volume;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// Primarily handles volume adjustment in gameplay.
    ///
    /// - If the user has mouse wheel disabled, only allow during break time or when holding alt. Also block scroll from parent handling.
    /// - Otherwise always allow, as per <see cref="GlobalScrollAdjustsVolume"/> implementation.
    /// </summary>
    internal partial class GameplayScrollWheelHandling : GlobalScrollAdjustsVolume
    {
        private Bindable<bool> mouseWheelDisabled = null!;

        [Resolved]
        private IGameplayClock gameplayClock { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            mouseWheelDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableWheel);
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            // During pause, allow global volume adjust regardless of settings.
            if (gameplayClock.IsPaused.Value)
                return base.OnScroll(e);

            // Block any parent handling of scroll if the user has asked for it (special case when holding "Alt").
            if (mouseWheelDisabled.Value && !e.AltPressed)
                return true;

            return base.OnScroll(e);
        }
    }
}
