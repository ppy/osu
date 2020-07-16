// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Configuration;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// Ensures screen is not suspended / dimmed while gameplay is active.
    /// </summary>
    public class ScreenSuspensionHandler : Component
    {
        private readonly GameplayClockContainer gameplayClockContainer;
        private Bindable<bool> isPaused;
        private readonly Bindable<bool> hasReplayLoaded;

        [Resolved]
        private GameHost host { get; set; }

        [Resolved]
        private SessionStatics statics { get; set; }

        public ScreenSuspensionHandler([NotNull] GameplayClockContainer gameplayClockContainer, Bindable<bool> hasReplayLoaded)
        {
            this.gameplayClockContainer = gameplayClockContainer ?? throw new ArgumentNullException(nameof(gameplayClockContainer));
            this.hasReplayLoaded = hasReplayLoaded.GetBoundCopy();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // This is the only usage game-wide of suspension changes.
            // Assert to ensure we don't accidentally forget this in the future.
            Debug.Assert(host.AllowScreenSuspension.Value);

            isPaused = gameplayClockContainer.IsPaused.GetBoundCopy();
            isPaused.BindValueChanged(paused =>
            {
                host.AllowScreenSuspension.Value = paused.NewValue;
                statics.Set(Static.DisableWindowsKey, !paused.NewValue && !hasReplayLoaded.Value);
            }, true);
            hasReplayLoaded.BindValueChanged(_ => isPaused.TriggerChange(), true);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            isPaused?.UnbindAll();
            hasReplayLoaded.UnbindAll();

            if (host != null)
            {
                host.AllowScreenSuspension.Value = true;
                statics.Set(Static.DisableWindowsKey, false);
            }
        }
    }
}
