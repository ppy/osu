// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Setup.Components.LabelledComponents
{
    public class LabelledSwitchButton : LabelledComponent<SwitchButton>
    {
        public LabelledSwitchButton()
            : base(true)
        {
        }

        protected override SwitchButton CreateComponent() => new SwitchButton();
    }
}
