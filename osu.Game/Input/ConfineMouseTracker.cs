// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Configuration;

namespace osu.Game.Input
{
    /// <summary>
    /// Connects <see cref="OsuSetting.ConfineMouseMode"/> with <see cref="FrameworkSetting.ConfineMouseMode"/>.
    /// If <see cref="OsuGame.LocalUserPlaying"/> is true, we should also confine the mouse cursor if it has been
    /// requested with <see cref="OsuConfineMouseMode.DuringGameplay"/>.
    /// </summary>
    public class ConfineMouseTracker : Component
    {
        private Bindable<ConfineMouseMode> frameworkConfineMode;
        private Bindable<WindowMode> frameworkWindowMode;

        private Bindable<OsuConfineMouseMode> osuConfineMode;
        private IBindable<bool> localUserPlaying;

        [BackgroundDependencyLoader]
        private void load(OsuGame game, FrameworkConfigManager frameworkConfigManager, OsuConfigManager osuConfigManager)
        {
            frameworkConfineMode = frameworkConfigManager.GetBindable<ConfineMouseMode>(FrameworkSetting.ConfineMouseMode);
            frameworkWindowMode = frameworkConfigManager.GetBindable<WindowMode>(FrameworkSetting.WindowMode);
            frameworkWindowMode.BindValueChanged(_ => updateConfineMode());

            osuConfineMode = osuConfigManager.GetBindable<OsuConfineMouseMode>(OsuSetting.ConfineMouseMode);
            localUserPlaying = game.LocalUserPlaying.GetBoundCopy();

            osuConfineMode.ValueChanged += _ => updateConfineMode();
            localUserPlaying.BindValueChanged(_ => updateConfineMode(), true);
        }

        private void updateConfineMode()
        {
            // confine mode is unavailable on some platforms
            if (frameworkConfineMode.Disabled)
                return;

            if (frameworkWindowMode.Value == WindowMode.Fullscreen)
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
