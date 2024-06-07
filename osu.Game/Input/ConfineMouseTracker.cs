// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Configuration;
using osu.Game.Screens.Play;

namespace osu.Game.Input
{
    /// <summary>
    /// Connects <see cref="OsuSetting.ConfineMouseMode"/> with <see cref="FrameworkSetting.ConfineMouseMode"/>.
    /// If <see cref="OsuGame.LocalUserPlaying"/> is true, we should also confine the mouse cursor if it has been
    /// requested with <see cref="OsuConfineMouseMode.DuringGameplay"/>.
    /// </summary>
    public partial class ConfineMouseTracker : Component
    {
        private Bindable<ConfineMouseMode> frameworkConfineMode;
        private Bindable<WindowMode> frameworkWindowMode;
        private Bindable<bool> frameworkMinimiseOnFocusLossInFullscreen;

        private Bindable<OsuConfineMouseMode> osuConfineMode;
        private IBindable<bool> localUserPlaying;

        [BackgroundDependencyLoader]
        private void load(ILocalUserPlayInfo localUserInfo, FrameworkConfigManager frameworkConfigManager, OsuConfigManager osuConfigManager)
        {
            frameworkConfineMode = frameworkConfigManager.GetBindable<ConfineMouseMode>(FrameworkSetting.ConfineMouseMode);
            frameworkWindowMode = frameworkConfigManager.GetBindable<WindowMode>(FrameworkSetting.WindowMode);
            frameworkMinimiseOnFocusLossInFullscreen = frameworkConfigManager.GetBindable<bool>(FrameworkSetting.MinimiseOnFocusLossInFullscreen);
            frameworkWindowMode.BindValueChanged(_ => updateConfineMode());
            frameworkMinimiseOnFocusLossInFullscreen.BindValueChanged(_ => updateConfineMode());

            osuConfineMode = osuConfigManager.GetBindable<OsuConfineMouseMode>(OsuSetting.ConfineMouseMode);
            localUserPlaying = localUserInfo.IsPlaying.GetBoundCopy();

            osuConfineMode.ValueChanged += _ => updateConfineMode();
            localUserPlaying.BindValueChanged(_ => updateConfineMode(), true);
        }

        private void updateConfineMode()
        {
            // confine mode is unavailable on some platforms
            if (frameworkConfineMode.Disabled)
                return;

            // override confine mode only when clicking outside the window minimises it.
            if (frameworkWindowMode.Value == WindowMode.Fullscreen && frameworkMinimiseOnFocusLossInFullscreen.Value)
            {
                frameworkConfineMode.Value = ConfineMouseMode.Fullscreen;
                return;
            }

            switch (osuConfineMode.Value)
            {
                case OsuConfineMouseMode.Never:
                    frameworkConfineMode.Value = ConfineMouseMode.Never;
                    break;

                case OsuConfineMouseMode.DuringGameplay:
                    frameworkConfineMode.Value = localUserPlaying.Value ? ConfineMouseMode.Always : ConfineMouseMode.Never;
                    break;

                case OsuConfineMouseMode.Always:
                    frameworkConfineMode.Value = ConfineMouseMode.Always;
                    break;
            }
        }
    }
}
