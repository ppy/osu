// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Screens;
using osu.Game.Screens.Play;

namespace osu.Desktop.Windows
{
    public class GameplayWinKeyHandler : Component
    {
        private Bindable<bool> allowScreenSuspension;
        private Bindable<bool> disableWinKey;

        private readonly OsuScreenStack screenStack;
        private GameHost host;

        private Type currentScreenType => screenStack.CurrentScreen?.GetType();

        public GameplayWinKeyHandler(OsuScreenStack stack)
        {
            screenStack = stack;
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host, OsuConfigManager config)
        {
            this.host = host;

            allowScreenSuspension = host.AllowScreenSuspension.GetBoundCopy();
            allowScreenSuspension.ValueChanged += toggleWinKey;

            disableWinKey = config.GetBindable<bool>(OsuSetting.GameplayDisableWinKey);
            disableWinKey.BindValueChanged(t => allowScreenSuspension.TriggerChange(), true);
        }

        private void toggleWinKey(ValueChangedEvent<bool> e)
        {
            var isPlayer = typeof(Player).IsAssignableFrom(currentScreenType) && currentScreenType != typeof(ReplayPlayer);

            if (!e.NewValue && disableWinKey.Value && isPlayer)
                host.InputThread.Scheduler.Add(WindowsKey.Disable);
            else
                host.InputThread.Scheduler.Add(WindowsKey.Enable);
        }
    }
}
