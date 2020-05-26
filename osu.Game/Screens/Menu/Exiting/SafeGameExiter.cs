// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Overlays;

namespace osu.Game.Screens.Menu.Exiting
{
    /// <summary>
    /// Represents a guarder component to ensure exiting from the game safely and intentionally
    /// without possibly corrupting game data or accidentally performing exit actions (by 0ms exit-hold-delay or otherwise).
    /// </summary>
    public class SafeGameExiter : Component
    {
        private readonly Action performExit;
        private readonly Action onExitBlock;

        private Bindable<float> holdDelay;

        [Resolved(canBeNull: true)]
        private DialogOverlay dialogOverlay { get; set; }

        private bool safeExitPerformed;

        /// <summary>
        /// Constructs a new <see cref="SafeGameExiter"/>
        /// </summary>
        /// <param name="performExit">An action for performing exit. Invoked by a <see cref="PerformSafeExit"/> call when exiting can be performed.</param>
        /// <param name="onExitBlock">Invoked if a <see cref="PerformSafeExit"/> call got blocked through one of the checks.</param>
        public SafeGameExiter(Action performExit, Action onExitBlock)
        {
            this.performExit = performExit;
            this.onExitBlock = onExitBlock;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            holdDelay = config.GetBindable<float>(OsuSetting.UIHoldActivationDelay);
        }

        /// <summary>
        /// Performs an exit from the game with safety and intention checks.
        /// </summary>
        /// <remarks>
        /// If called and returned <c>true</c> at least once, all calls afterwards will early-return with <c>true</c>.
        /// </remarks>
        /// <param name="enforceConfirmation">Whether to enforce showing the confirmation dialog.</param>
        /// <returns>Whether to continue exiting if it is.</returns>
        public bool PerformSafeExit(bool enforceConfirmation)
        {
            if (safeExitPerformed)
                return true;

            if (dialogOverlay == null)
            {
                onSafeExitPassed();
                return true;
            }

            // No need to push confirmation dialogs if still at the cannot-exit dialog.
            if (!(dialogOverlay.CurrentDialog is CannotExitDialog))
            {
                if (holdDelay.Value == 0 || enforceConfirmation)
                {
                    if (dialogOverlay.CurrentDialog is ConfirmExitDialog exitDialog)
                        exitDialog.Buttons.First().Click();
                    else
                        dialogOverlay.Push(new ConfirmExitDialog(onSafeExitPassed, onExitBlock));

                    return false;
                }
            }

            onSafeExitPassed();
            return true;
        }

        private void onSafeExitPassed()
        {
            safeExitPerformed = true;
            performExit?.Invoke();
        }
    }
}
