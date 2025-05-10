// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input;

namespace osu.Game.Graphics.UserInterface
{
    public partial class OsuNumberBox : OsuTextBox
    {
        public OsuNumberBox()
        {
            InputProperties = new TextInputProperties(TextInputType.Number, false);

            SelectAllOnFocus = true;
        }
    }
}
