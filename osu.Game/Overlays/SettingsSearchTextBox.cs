// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays
{
    public partial class SettingsSearchTextBox : SeekLimitedSearchTextBox
    {
        protected override void OnFocus(FocusEvent e)
        {
            base.OnFocus(e);
            SelectAll();
        }
    }
}
