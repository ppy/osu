// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Screens;
using osu.Game.Screens.Play;

namespace osu.Game.Utils
{
    public static class MobileUtils
    {
        /// <summary>
        /// Determines the correct <see cref="Orientation"/> state which a mobile device should be put into for the given information.
        /// </summary>
        /// <param name="userPlayInfo">Information about whether the user is currently playing.</param>
        /// <param name="currentScreen">The current screen which the user is at.</param>
        /// <param name="isTablet">Whether the user is playing on a mobile tablet device instead of a phone.</param>
        public static Orientation GetOrientation(ILocalUserPlayInfo userPlayInfo, IOsuScreen currentScreen, bool isTablet)
        {
            bool lockCurrentOrientation = userPlayInfo.PlayingState.Value == LocalUserPlayingState.Playing;
            bool lockToPortraitOnPhone = currentScreen.RequiresPortraitOrientation;

            if (lockToPortraitOnPhone && !isTablet)
                return Orientation.Portrait;

            if (lockCurrentOrientation)
                return Orientation.Locked;

            return Orientation.Default;
        }

        public enum Orientation
        {
            /// <summary>
            /// Lock the game orientation.
            /// </summary>
            Locked,

            /// <summary>
            /// Lock the game to portrait orientation (does not include upside-down portrait).
            /// </summary>
            Portrait,

            /// <summary>
            /// Use the application's default settings.
            /// </summary>
            Default,
        }
    }
}
