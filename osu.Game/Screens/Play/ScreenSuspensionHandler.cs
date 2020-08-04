// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
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
    public class ScreenSuspensionHandler : Component
    {
        private readonly GameplayClockContainer gameplayClockContainer;
        private Bindable<bool> isPaused;

        [Resolved]
        private GameHost host { get; set; }

        public ScreenSuspensionHandler([NotNull] GameplayClockContainer gameplayClockContainer)
        {
            this.gameplayClockContainer = gameplayClockContainer ?? throw new ArgumentNullException(nameof(gameplayClockContainer));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // This is the only usage game-wide of suspension changes.
            // Assert to ensure we don't accidentally forget this in the future.
            Debug.Assert(host.AllowScreenSuspension.Value);

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
