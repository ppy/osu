// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
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

        private const float switch_horizontal_offset = 15;
        private const float switch_vertical_offset = 10;

        public Bindable<bool> Current { get; } = new Bindable<bool>();

        [BackgroundDependencyLoader]
        private void load()
        {
            Current.BindTo(switchButton.Current);
        }

        protected override Drawable CreateComponent() => switchButton = new SwitchButton
        {
            Anchor = Anchor.TopRight,
            Origin = Anchor.TopRight,
            Position = new Vector2(-switch_horizontal_offset, switch_vertical_offset),
        };
    }
}
