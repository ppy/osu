// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Mobile
{
    public enum GameOrientation
    {
        /// <summary>
        /// Lock the game orientation.
        /// </summary>
        Locked,

        /// <summary>
        /// Display the game in regular portrait orientation.
        /// </summary>
        Portrait,

        /// <summary>
        /// Display the game in landscape-right orientation.
        /// </summary>
        Landscape,

        /// <summary>
        /// Display the game in landscape-right/landscape-left orientations.
        /// </summary>
        FullLandscape,

        /// <summary>
        /// Display the game in portrait/portrait-upside-down orientations.
        /// This is exclusive to tablet mobile devices.
        /// </summary>
        FullPortrait,
    }
}
