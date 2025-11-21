// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Footer
{
    public partial class ScreenStackFooter : CompositeDrawable
    {
        /// <summary>
        /// Called when logo tracking begins, intended to bring the osu! logo to the frontmost visually.
        /// </summary>
        public Action<bool>? RequestLogoInFront { private get; init; }

        /// <summary>
        /// The back button was pressed.
        /// </summary>
        public Action? BackButtonPressed { private get; init; }

        /// <summary>
        /// The (legacy) back button.
        /// </summary>
        public readonly BackButton BackButton;

        /// <summary>
        /// The footer.
        /// </summary>
        public readonly ScreenFooter Footer;

        /// <summary>
        /// Whether the legacy back button is currently displayed.
        /// </summary>
        private readonly IBindable<bool> backButtonVisibility = new BindableBool();

        private readonly ScreenStackTracker screenTracker;

        public ScreenStackFooter(ScreenStack screenStack, ScreenFooter.BackReceptor? backReceptor = null)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                BackButton = new BackButton(backReceptor)
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Action = () => BackButtonPressed?.Invoke(),
                },
                Footer = new ScreenFooter(backReceptor)
                {
                    RequestLogoInFront = v => RequestLogoInFront?.Invoke(v),
                    BackButtonPressed = () => BackButtonPressed?.Invoke()
                }
            };

            screenTracker = new ScreenStackTracker(screenStack);
            screenTracker.ScreenChanged += onScreenChanged;

            backButtonVisibility.ValueChanged += onBackButtonVisibilityChanged;
        }

        private void onScreenChanged(IScreen lastScreen, IScreen newScreen)
        {
            unbindScreen(lastScreen);
            bindScreen(newScreen);
        }

        private void onBackButtonVisibilityChanged(ValueChangedEvent<bool> visible)
        {
            if (visible.NewValue)
                BackButton.Show();
            else
                BackButton.Hide();
        }

        private void unbindScreen(IScreen screen)
        {
            if (screen is not OsuScreen osuScreen)
                return;

            backButtonVisibility.UnbindFrom(osuScreen.BackButtonVisibility);
        }

        private void bindScreen(IScreen screen)
        {
            if (screen is not OsuScreen osuScreen)
            {
                ((BindableBool)backButtonVisibility).Value = true;

                Footer.SetButtons([]);
                Footer.Hide();
                return;
            }

            if (osuScreen.ShowFooter)
            {
                // the legacy back button should never display while the new footer is in use, as it
                // contains its own local back button.
                ((BindableBool)backButtonVisibility).Value = false;

                Footer.Show();

                if (osuScreen.IsLoaded)
                    updateFooterButtons();
                else
                {
                    // ensure the current buttons are immediately disabled on screen change (so they can't be pressed).
                    Footer.SetButtons([]);

                    osuScreen.OnLoadComplete += _ => updateFooterButtons();
                }

                void updateFooterButtons()
                {
                    var buttons = osuScreen.CreateFooterButtons();

                    osuScreen.LoadComponentsAgainstScreenDependencies(buttons);

                    Footer.SetButtons(buttons);
                    Footer.Show();
                }
            }
            else
            {
                backButtonVisibility.BindTo(osuScreen.BackButtonVisibility);

                Footer.SetButtons([]);
                Footer.Hide();
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            screenTracker.Dispose();
        }

        /// <summary>
        /// Recursively represents a single screen stack and any nested subscreen stack.
        /// </summary>
        private class ScreenStackTracker : IDisposable
        {
            /// <summary>
            /// Invoked when the leading screen changes.
            /// </summary>
            /// <remarks>
            /// This differs from <see cref="ScreenStack.ScreenPushed"/> and <see cref="ScreenStack.ScreenExited"/>
            /// because <c>lastScreen</c> and <c>newScreen</c> may be subscreens of the current screen stack.
            /// <br />
            /// As such, no assumptions may be made as to the relation of screens to this entry's <see cref="ScreenStack"/>.
            /// </remarks>
            public event ScreenChangedDelegate? ScreenChanged;

            /// <summary>
            /// The screen stack tracked by this entry.
            /// </summary>
            private readonly ScreenStack stack;

            /// <summary>
            /// An entry corresponding to the subscreen stack of the current screen, if any.
            /// </summary>
            private ScreenStackTracker? subScreenTracker;

            /// <summary>
            /// The screen which should be bound to the screen footer - the most nested subscreen.
            /// </summary>
            private IScreen leadingScreen => subScreenTracker?.leadingScreen ?? stack.CurrentScreen;

            public ScreenStackTracker(ScreenStack stack)
            {
                this.stack = stack;

                stack.ScreenPushed += onParentScreenChanged;
                stack.ScreenExited += onParentScreenChanged;
            }

            private void onParentScreenChanged(IScreen lastScreen, IScreen newScreen)
            {
                // The screen which we will be UNBINDING from the screen footer later on.
                IScreen lastLeadingScreen = subScreenTracker?.leadingScreen ?? lastScreen;

                // Subscreens are attached to a parent screen, so when the parent changes the subscreen must also.
                subScreenTracker?.Dispose();
                subScreenTracker = null;

                // Check if we've switched to a screen that has a subscreen.
                if (newScreen is IHasSubScreenStack newStack)
                {
                    subScreenTracker = new ScreenStackTracker(newStack.SubScreenStack);
                    subScreenTracker.ScreenChanged += onSubScreenScreenChanged;
                }

                ScreenChanged?.Invoke(lastLeadingScreen, leadingScreen);
            }

            private void onSubScreenScreenChanged(IScreen lastScreen, IScreen newScreen)
            {
                ScreenChanged?.Invoke(lastScreen, newScreen);
            }

            public void Dispose()
            {
                stack.ScreenPushed -= onParentScreenChanged;
                stack.ScreenExited -= onParentScreenChanged;

                if (subScreenTracker != null)
                {
                    subScreenTracker.ScreenChanged -= onSubScreenScreenChanged;
                    subScreenTracker.Dispose();
                }
            }
        }
    }
}
