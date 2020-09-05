// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Configuration;

namespace osu.Desktop.Windows
{
    public class GameplayWinKeyBlocker : Component
    {
        private Bindable<bool> allowScreenSuspension;
        private Bindable<bool> disableWinKey;

        private GameHost host;

        [BackgroundDependencyLoader]
        private void load(GameHost host, OsuConfigManager config)
        {
            this.host = host;

            allowScreenSuspension = host.AllowScreenSuspension.GetBoundCopy();
            allowScreenSuspension.BindValueChanged(_ => updateBlocking());

            disableWinKey = config.GetBindable<bool>(OsuSetting.GameplayDisableWinKey);
            disableWinKey.BindValueChanged(_ => updateBlocking(), true);
        }

        private void updateBlocking()
        {
            bool shouldDisable = disableWinKey.Value && !allowScreenSuspension.Value;

            if (shouldDisable)
                host.InputThread.Scheduler.Add(WindowsKey.Disable);
            else
                host.InputThread.Scheduler.Add(WindowsKey.Enable);
        }
    }
}
