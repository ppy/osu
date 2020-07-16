// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Configuration;

namespace osu.Desktop.Windows
{
    public class GameplayWinKeyHandler : Component
    {
        private Bindable<bool> disableWinKey;
        private Bindable<bool> disableWinKeySetting;

        private GameHost host;

        [BackgroundDependencyLoader]
        private void load(GameHost host, OsuConfigManager config, SessionStatics statics)
        {
            this.host = host;

            disableWinKey = statics.GetBindable<bool>(Static.DisableWindowsKey);
            disableWinKey.ValueChanged += toggleWinKey;

            disableWinKeySetting = config.GetBindable<bool>(OsuSetting.GameplayDisableWinKey);
            disableWinKeySetting.BindValueChanged(t => disableWinKey.TriggerChange(), true);
        }

        private void toggleWinKey(ValueChangedEvent<bool> e)
        {
            if (e.NewValue && disableWinKeySetting.Value)
                host.InputThread.Scheduler.Add(WindowsKey.Disable);
            else
                host.InputThread.Scheduler.Add(WindowsKey.Enable);
        }
    }
}
