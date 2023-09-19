// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A <see cref="SearchTextBox"/> which does not handle left/right arrow keys for seeking.
    /// </summary>
    public partial class SeekLimitedSearchTextBox : BasicSearchTextBox
    {
        public override bool HandleLeftRightArrows => false;
    }
}
