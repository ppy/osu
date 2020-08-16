// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    /// Connects <see cref="OsuSetting.ConfineMouseMode"/> with <see cref="FrameworkSetting.ConfineMouseMode"/>
    /// while providing a property for <see cref="Player"/> to indicate whether gameplay is currently active.
    /// </summary>
    public class ConfineMouseTracker : Component
    {
        private Bindable<ConfineMouseMode> frameworkConfineMode;
        private Bindable<OsuConfineMouseMode> osuConfineMode;

        private bool gameplayActive;

        /// <summary>
        /// Indicates whether osu! is currently considered "in gameplay" for the
        /// purposes of <see cref="OsuConfineMouseMode.DuringGameplay"/>.
        /// </summary>
        public bool GameplayActive
        {
            get => gameplayActive;
            set
            {
                if (gameplayActive == value)
                    return;

                gameplayActive = value;
                updateConfineMode();
            }
        }

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager frameworkConfigManager, OsuConfigManager osuConfigManager)
        {
            frameworkConfineMode = frameworkConfigManager.GetBindable<ConfineMouseMode>(FrameworkSetting.ConfineMouseMode);
            osuConfineMode = osuConfigManager.GetBindable<OsuConfineMouseMode>(OsuSetting.ConfineMouseMode);
            osuConfineMode.BindValueChanged(_ => updateConfineMode(), true);
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

                case OsuConfineMouseMode.DuringGameplay:
                    frameworkConfineMode.Value = GameplayActive ? ConfineMouseMode.Always : ConfineMouseMode.Never;
                    break;

                case OsuConfineMouseMode.Always:
                    frameworkConfineMode.Value = ConfineMouseMode.Always;
                    break;
            }
        }
    }
}
