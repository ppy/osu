// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Screens.Footer
{
    /// <summary>
    /// Describes the content that an <see cref="IOsuScreen"/> can command the global <see cref="ScreenFooter"/> to show.
    /// </summary>
    public record ScreenFooterContent
    {
        /// <summary>
        /// The mode of displaying the global footer.
        /// </summary>
        public FooterMode Mode { get; private init; }

        /// <summary>
        /// Whether the back button should be shown.
        /// </summary>
        public bool BackButton { get; init; }

        /// <summary>
        /// The <see cref="ScreenFooterButton"/>s - if any - to show on the global <see cref="ScreenFooter"/>.
        /// Can pass <see langword="null"/> for no buttons.
        /// </summary>
        /// <remarks>
        /// Ignored if <see cref="Mode"/> is <see cref="FooterMode.Legacy"/>.
        /// </remarks>
        public Func<IEnumerable<ScreenFooterButton>>? LeftButtons { get; private init; }

        private ScreenFooterContent()
        {
        }

        /// <summary>
        /// Show no footer.
        /// </summary>
        public static ScreenFooterContent None => new ScreenFooterContent
        {
            Mode = FooterMode.None,
        };

        /// <summary>
        /// Allows to show single two-layered back button, similar to stable's default. Not skinnable.
        /// </summary>
        /// <param name="showBackButton">Whether to show the back button.</param>
        public static ScreenFooterContent Legacy(bool showBackButton)
        {
            return new ScreenFooterContent
            {
                Mode = FooterMode.Legacy,
                BackButton = showBackButton,
            };
        }

        /// <summary>
        /// Allows to show a global, persistent footer with capability to show screen-specific buttons and overlay-specific content.
        /// Not skinnable. Uses the argon UI aesthetic.
        /// </summary>
        /// <param name="leftButtons">The screen-specific buttons to show on the left side of the footer.</param>
        public static ScreenFooterContent Argon(Func<IEnumerable<ScreenFooterButton>>? leftButtons = null)
        {
            return new ScreenFooterContent
            {
                Mode = FooterMode.Argon,
                // TODO: The argon global screen footer does not currently support hiding the back button, so it is forced on.
                // Make this work when required.
                BackButton = true,
                LeftButtons = leftButtons
            };
        }

        public enum FooterMode
        {
            /// <summary>
            /// Show no footer.
            /// </summary>
            None,

            /// <summary>
            /// Allows to show a single two-layered back button, similar to stable's default. Not skinnable.
            /// </summary>
            // TODO: possible attachment point for making back button skinning work across the entire game
            Legacy,

            /// <summary>
            /// Allows to show a global, persistent footer with capability to show screen-specific buttons and overlay-specific content.
            /// Not skinnable. Uses the argon UI aesthetic.
            /// </summary>
            Argon,
        }
    }
}
