// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Platform;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// Ensures screen is not suspended / dimmed while gameplay is active.
    /// </summary>
    public partial class ScreenSuspensionHandler : Component
    {
        private readonly GameplayClockContainer gameplayClockContainer;
        private IBindable<bool> isPaused;

        private readonly Bindable<bool> disableSuspensionBindable = new Bindable<bool>();

        [Resolved]
        private GameHost host { get; set; }

        public ScreenSuspensionHandler([NotNull] GameplayClockContainer gameplayClockContainer)
        {
            this.gameplayClockContainer = gameplayClockContainer ?? throw new ArgumentNullException(nameof(gameplayClockContainer));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            isPaused = gameplayClockContainer.IsPaused.GetBoundCopy();
            isPaused.BindValueChanged(paused =>
            {
                if (paused.NewValue)
                    host.AllowScreenSuspension.RemoveSource(disableSuspensionBindable);
                else
                    host.AllowScreenSuspension.AddSource(disableSuspensionBindable);
            }, true);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            isPaused?.UnbindAll();
            host?.AllowScreenSuspension.RemoveSource(disableSuspensionBindable);
        }
    }
}
