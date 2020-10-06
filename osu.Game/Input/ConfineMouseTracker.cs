// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Configuration;
using osu.Game.Overlays;

namespace osu.Game.Input
{
    /// <summary>
    /// Connects <see cref="OsuSetting.ConfineMouseMode"/> with <see cref="FrameworkSetting.ConfineMouseMode"/>,
    /// while binding <see cref="OsuGame.OverlayActivationMode"/>.
    /// It is assumed that while overlay activation is <see cref="OverlayActivation.Disabled"/>, we should also confine the
    /// mouse cursor if it has been requested with <see cref="OsuConfineMouseMode.WhenOverlaysDisabled"/>.
    /// </summary>
    public class ConfineMouseTracker : Component
    {
        private Bindable<ConfineMouseMode> frameworkConfineMode;
        private Bindable<OsuConfineMouseMode> osuConfineMode;
        private IBindable<OverlayActivation> overlayActivationMode;

        [BackgroundDependencyLoader]
        private void load(OsuGame game, FrameworkConfigManager frameworkConfigManager, OsuConfigManager osuConfigManager)
        {
            frameworkConfineMode = frameworkConfigManager.GetBindable<ConfineMouseMode>(FrameworkSetting.ConfineMouseMode);
            osuConfineMode = osuConfigManager.GetBindable<OsuConfineMouseMode>(OsuSetting.ConfineMouseMode);
            osuConfineMode.ValueChanged += _ => updateConfineMode();
            overlayActivationMode = game.OverlayActivationMode.GetBoundCopy();
            overlayActivationMode.BindValueChanged(_ => updateConfineMode(), true);
        }

        private void updateConfineMode()
        {
            switch (osuConfineMode.Value)
            {
                case OsuConfineMouseMode.Never:
                    frameworkConfineMode.Value = ConfineMouseMode.Never;
                    break;

                case OsuConfineMouseMode.Fullscreen:
                    frameworkConfineMode.Value = ConfineMouseMode.Fullscreen;
                    break;

                case OsuConfineMouseMode.WhenOverlaysDisabled:
                    frameworkConfineMode.Value = overlayActivationMode?.Value == OverlayActivation.Disabled ? ConfineMouseMode.Always : ConfineMouseMode.Never;
                    break;

                case OsuConfineMouseMode.Always:
                    frameworkConfineMode.Value = ConfineMouseMode.Always;
                    break;
            }
        }
    }
}
