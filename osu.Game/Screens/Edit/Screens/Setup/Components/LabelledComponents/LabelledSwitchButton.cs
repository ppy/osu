// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Screens.Edit.Components;

namespace osu.Game.Screens.Edit.Screens.Setup.Components.LabelledComponents
{
    public class LabelledSwitchButton : LabelledComponent, IHasCurrentValue<bool>
    {
        private SwitchButton switchButton;

        public Bindable<bool> Current { get; } = new Bindable<bool>();

        [BackgroundDependencyLoader]
        private void load()
        {
            Current.BindTo(switchButton.Current);
        }

        protected override Drawable CreateComponent() => switchButton = new SwitchButton();
    }
}
