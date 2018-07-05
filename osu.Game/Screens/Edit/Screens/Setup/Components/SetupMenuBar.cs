// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Game.Screens.Edit.Screens;

namespace osu.Game.Screens.Edit.Screens.Setup.Components
{
    public class SetupMenuBar : OsuMenu
    {
        public readonly Bindable<SetupScreenMode> Mode = new Bindable<SetupScreenMode>();

        public SetupMenuBar()
            : base(Direction.Horizontal, true)
        {
            RelativeSizeAxes = Axes.X;

            MaskingContainer.CornerRadius = 0;
            ItemsContainer.Padding = new MarginPadding { Left = 60 };
            BackgroundColour = OsuColour.FromHex("1c2125");

            SetupScreenSelectionTabControl tabControl;
            AddRangeInternal(new Drawable[]
            {
                tabControl = new SetupScreenSelectionTabControl
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    X = 60
                }
            });

            tabControl.Current.Value = SetupScreenMode.General;
            Mode.BindTo(tabControl.Current);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Mode.TriggerChange();
        }
    }
}
