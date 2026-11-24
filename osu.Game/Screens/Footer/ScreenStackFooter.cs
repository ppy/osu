// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
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
        /// Describes the footer content to be displayed, as requested by the current <see cref="IOsuScreen"/>.
        /// </summary>
        private readonly IBindable<ScreenFooterContent> footerContent = new Bindable<ScreenFooterContent>(ScreenFooterContent.None);

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

            footerContent.BindValueChanged(_ => Scheduler.AddOnce(footerContentChanged));
        }

        private void onScreenChanged(IScreen lastScreen, IScreen newScreen)
        {
            unbindScreen(lastScreen);
            bindScreen(newScreen);
        }

        private void bindScreen(IScreen screen)
        {
            if (screen is not OsuScreen osuScreen)
            {
                ((Bindable<ScreenFooterContent>)footerContent).Value = ScreenFooterContent.Legacy(showBackButton: true);

                Footer.SetButtons([], [], Direction.Vertical);
                Footer.Hide();
                return;
            }

            footerContent.BindTo(osuScreen.FooterContent);
        }

        private void unbindScreen(IScreen screen)
        {
            if (screen is not OsuScreen osuScreen)
                return;

            footerContent.UnbindFrom(osuScreen.FooterContent);
        }

        private void footerContentChanged()
        {
            switch (footerContent.Value.Mode)
            {
                case ScreenFooterContent.FooterMode.None:
                    Footer.SetButtons([], [], Direction.Vertical);
                    Footer.Hide();
                    BackButton.Hide();
                    break;

                case ScreenFooterContent.FooterMode.Legacy:
                    Footer.SetButtons([], [], Direction.Vertical);
                    Footer.Hide();
                    BackButton.State.Value = footerContent.Value.BackButton ? Visibility.Visible : Visibility.Hidden;
                    break;

                case ScreenFooterContent.FooterMode.Argon:
                    BackButton.State.Value = Visibility.Hidden;
                    Footer.Show();

                    var osuScreen = screenTracker.LeadingScreen as OsuScreen;
                    Debug.Assert(osuScreen != null);

                    bool anyOldButtons = Footer.HasButtons;
                    // the condition below is slightly indirect.
                    // the point is to check if there are any new buttons to show.
                    // checking if the actions to create the buttons are present isn't the same thing - they could return empty enumerables after all.
                    // however, the buttons cannot be reliably constructed at this time because the screen is possibly not loaded yet,
                    // and so buttons created at this time may not be hooked up to screen contents properly.
                    // therefore settle for the next best thing. consumers who set the actions to return empty arrays have only themselves to blame.
                    bool anyNewButtons = footerContent.Value.LeftButtons != null || footerContent.Value.RightButtons != null;

                    var direction = anyOldButtons && anyNewButtons ? Direction.Horizontal : Direction.Vertical;

                    if (osuScreen.IsLoaded)
                        updateFooterButtons(osuScreen, direction);
                    else
                    {
                        // ensure the current buttons are immediately disabled on screen change (so they can't be pressed).
                        Footer.SetButtons([], [], direction);

                        osuScreen.OnLoadComplete += _ => updateFooterButtons(osuScreen, direction);
                    }

                    break;
            }

            void updateFooterButtons(OsuScreen osuScreen, Direction direction)
            {
                var leftButtons = footerContent.Value.LeftButtons?.Invoke().ToArray() ?? [];
                var rightButtons = footerContent.Value.RightButtons?.Invoke().ToArray() ?? [];

                osuScreen.LoadComponentsAgainstScreenDependencies(leftButtons);
                osuScreen.LoadComponentsAgainstScreenDependencies(rightButtons);

                Footer.SetButtons(leftButtons, rightButtons, direction);
                Footer.Show();
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
            public IScreen LeadingScreen => subScreenTracker?.LeadingScreen ?? stack.CurrentScreen;

            public ScreenStackTracker(ScreenStack stack)
            {
                this.stack = stack;

                stack.ScreenPushed += onParentScreenChanged;
                stack.ScreenExited += onParentScreenChanged;
            }

            private void onParentScreenChanged(IScreen lastScreen, IScreen newScreen)
            {
                // The screen which we will be UNBINDING from the screen footer later on.
                IScreen lastLeadingScreen = subScreenTracker?.LeadingScreen ?? lastScreen;

                // Subscreens are attached to a parent screen, so when the parent changes the subscreen must also.
                subScreenTracker?.Dispose();
                subScreenTracker = null;

                // Check if we've switched to a screen that has a subscreen.
                if (newScreen is IHasSubScreenStack newStack)
                {
                    subScreenTracker = new ScreenStackTracker(newStack.SubScreenStack);
                    subScreenTracker.ScreenChanged += onSubScreenScreenChanged;
                }

                ScreenChanged?.Invoke(lastLeadingScreen, LeadingScreen);
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
