// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Footer
{
    /// <summary>
    /// Interface used by <see cref="ScreenFooter"/> to manage transitions of buttons.
    /// </summary>
    public interface IScreenFooterButton
    {
        void AppearFromLeft(double delay);
        void AppearFromBottom(double delay);
        void DisappearToRight(double delay, bool expire);
        void DisappearToBottom(double delay, bool expire);
    }
}
