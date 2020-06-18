// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Platform;

namespace osu.Game.Screens.Play
{
    internal class ScreenSuspensionHandler : Component
    {
        private readonly GameplayClockContainer gameplayClockContainer;
        private Bindable<bool> isPaused;

        [Resolved]
        private GameHost host { get; set; }

        public ScreenSuspensionHandler(GameplayClockContainer gameplayClockContainer)
        {
            this.gameplayClockContainer = gameplayClockContainer;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            isPaused = gameplayClockContainer.IsPaused.GetBoundCopy();
            isPaused.BindValueChanged(paused => host.AllowScreenSuspension.Value = paused.NewValue, true);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            isPaused?.UnbindAll();

            if (host != null)
                host.AllowScreenSuspension.Value = true;
        }
    }
}
