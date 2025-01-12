// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online.Chat
{
    public enum LinkWarnMode
    {
        /// <summary>
        /// Will show a dialog when opening a URL that is not on a trusted domain.
        /// </summary>
        Default,

        /// <summary>
        /// Will always show a dialog when opening a URL.
        /// </summary>
        AlwaysWarn,

        /// <summary>
        /// Will never show a dialog when opening a URL.
        /// </summary>
        NeverWarn,
    }
}
