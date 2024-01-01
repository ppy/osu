// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens;
using osu.Game.Screens.Menu;

namespace osu.Game
{
    internal partial class PerformFromMenuRunner : Component
    {
        private readonly Action<IScreen> finalAction;
        private readonly Type[] validScreens;
        private readonly Func<IScreen> getCurrentScreen;

        [Resolved]
        private INotificationOverlay notifications { get; set; }

        [Resolved]
        private IDialogOverlay dialogOverlay { get; set; }

        [Resolved(canBeNull: true)]
        private OsuGame game { get; set; }

        private readonly ScheduledDelegate task;

        private PopupDialog lastEncounteredDialog;
        private IScreen lastEncounteredDialogScreen;

        /// <summary>
        /// Perform an action only after returning to a specific screen as indicated by <paramref name="validScreens"/>.
        /// Eagerly tries to exit the current screen until it succeeds.
        /// </summary>
        /// <param name="finalAction">The action to perform once we are in the correct state.</param>
        /// <param name="validScreens">An optional collection of valid screen types. If any of these screens are already current we can perform the action immediately, else the first valid parent will be made current before performing the action. <see cref="MainMenu"/> is used if not specified.</param>
        /// <param name="getCurrentScreen">A function to retrieve the currently displayed game screen.</param>
        public PerformFromMenuRunner(Action<IScreen> finalAction, IEnumerable<Type> validScreens, Func<IScreen> getCurrentScreen)
        {
            validScreens ??= Enumerable.Empty<Type>();
            validScreens = validScreens.Append(typeof(MainMenu));

            this.finalAction = finalAction;
            this.validScreens = validScreens.ToArray();
            this.getCurrentScreen = getCurrentScreen;

            Scheduler.Add(task = new ScheduledDelegate(checkCanComplete, 0, 200));
        }

        /// <summary>
        /// Cancel this runner from running.
        /// </summary>
        public void Cancel()
        {
            task.Cancel();
            Expire();
        }

        private void checkCanComplete()
        {
            // find closest valid target
            IScreen current = getCurrentScreen();

            if (current == null)
                return;

            // a dialog may be blocking the execution for now.
            if (checkForDialog(current)) return;

            game?.CloseAllOverlays(false);

            findValidTarget(current);
        }

        private bool findValidTarget(IScreen current)
        {
            var type = current.GetType();

            // check if we are already at a valid target screen.
            if (validScreens.Any(t => t.IsAssignableFrom(type)))
            {
                if (!((Drawable)current).IsLoaded)
                    // wait until screen is loaded before invoking action.
                    return true;

                finalAction(current);
                Cancel();
                return true;
            }

            while (current != null)
            {
                // if this has a sub stack, recursively check the screens within it.
                if (current is IHasSubScreenStack currentSubScreen)
                {
                    var nestedCurrent = currentSubScreen.SubScreenStack.CurrentScreen;

                    if (nestedCurrent != null)
                    {
                        // should be correct in theory, but currently untested/unused in existing implementations.
                        // note that calling findValidTarget actually performs the final operation.
                        if (findValidTarget(nestedCurrent))
                            return true;
                    }
                }

                if (validScreens.Any(t => t.IsAssignableFrom(type)))
                {
                    current.MakeCurrent();
                    return true;
                }

                current = current.GetParentScreen();
                type = current?.GetType();
            }

            return false;
        }

        /// <summary>
        /// Check whether there is currently a dialog requiring user interaction.
        /// </summary>
        /// <param name="current"></param>
        /// <returns>Whether a dialog blocked interaction.</returns>
        private bool checkForDialog(IScreen current)
        {
            // An exit process may traverse multiple levels.
            // When checking for dismissing dialogs, let's also consider sub screens.
            while (current is IHasSubScreenStack currentWithSubScreenStack)
            {
                var nestedCurrent = currentWithSubScreenStack.SubScreenStack.CurrentScreen;

                if (nestedCurrent == null)
                    break;

                current = nestedCurrent;
            }

            var currentDialog = dialogOverlay.CurrentDialog;

            if (lastEncounteredDialog != null)
            {
                if (lastEncounteredDialog == currentDialog)
                    // still waiting on user interaction
                    return true;

                if (lastEncounteredDialogScreen != current)
                {
                    // a dialog was previously encountered but has since been dismissed.
                    // if the screen changed, the user likely confirmed an exit dialog and we should continue attempting the action.
                    lastEncounteredDialog = null;
                    lastEncounteredDialogScreen = null;
                    return false;
                }

                // the last dialog encountered has been dismissed but the screen has not changed, abort.
                Cancel();

                Logger.Log("An action was interrupted due to a dialog being displayed.", level: LogLevel.Debug);

                return true;
            }

            if (currentDialog == null)
                return false;

            // a new dialog was encountered.
            lastEncounteredDialog = currentDialog;
            lastEncounteredDialogScreen = current;
            return true;
        }
    }
}
