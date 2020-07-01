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
        private Bindable<bool> winKeyEnabled;
        private Bindable<bool> disableWinKey;

        private GameHost host;

        [BackgroundDependencyLoader]
        private void load(GameHost host, OsuConfigManager config)
        {
            this.host = host;

            winKeyEnabled = host.AllowScreenSuspension.GetBoundCopy();
            winKeyEnabled.ValueChanged += toggleWinKey;

            disableWinKey = config.GetBindable<bool>(OsuSetting.GameplayDisableWinKey);
            disableWinKey.BindValueChanged(t => winKeyEnabled.TriggerChange());
        }

        private void toggleWinKey(ValueChangedEvent<bool> e)
        {
            if (!e.NewValue && disableWinKey.Value)
                host.InputThread.Scheduler.Add(WindowsKey.Disable);
            else
                host.InputThread.Scheduler.Add(WindowsKey.Enable);
        }
    }
}
