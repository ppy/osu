// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Select;

namespace osu.Game.Overlays
{
    [Cached]
    internal interface IOverlayManager
    {
        /// <summary>
        /// Whether overlays should be able to be opened game-wide. Value is sourced from the current active screen.
        /// </summary>
        IBindable<OverlayActivation> OverlayActivationMode { get; }

        /// <summary>
        /// Registers a blocking <see cref="OverlayContainer"/> that was not created by <see cref="OsuGame"/> itself for later use.
        /// </summary>
        /// <remarks>
        /// The goal of this method is to allow child screens, like <see cref="SongSelect"/> to register their own full-screen blocking overlays
        /// with background dim.
        /// In those cases, for the dim to work correctly, the overlays need to be added at a game level directly, rather as children of the screens.
        /// </remarks>
        /// <returns>
        /// An <see cref="IDisposable"/> that should be disposed of when the <paramref name="overlayContainer"/> should be unregistered.
        /// Disposing of this <see cref="IDisposable"/> will automatically expire the <paramref name="overlayContainer"/>.
        /// </returns>
        IDisposable RegisterBlockingOverlay(OverlayContainer overlayContainer);

        /// <summary>
        /// Should be called when <paramref name="overlay"/> has been shown and should begin blocking background input.
        /// </summary>
        void ShowBlockingOverlay(OverlayContainer overlay);

        /// <summary>
        /// Should be called when a blocking <paramref name="overlay"/> has been hidden and should stop blocking background input.
        /// </summary>
        void HideBlockingOverlay(OverlayContainer overlay);
    }
}
