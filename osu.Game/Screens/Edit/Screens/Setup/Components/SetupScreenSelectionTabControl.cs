// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK;

namespace osu.Game.Screens.Edit.Screens.Setup.Components
{
    public class SetupScreenSelectionTabControl : OsuTabControl<SetupScreenMode>
    {
        public SetupScreenSelectionTabControl()
        {
            AutoSizeAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;

            TabContainer.RelativeSizeAxes &= ~Axes.X;
            TabContainer.AutoSizeAxes = Axes.X;
            TabContainer.Padding = new MarginPadding();
            TabContainer.Spacing = new Vector2(15f, 0f);

            Add(new Box
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                RelativeSizeAxes = Axes.X,
                Height = 1,
                Colour = Color4.White.Opacity(0.2f),
            });

            Current.Value = SetupScreenMode.General;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.Blue;
        }

        protected override Dropdown<SetupScreenMode> CreateDropdown() => null;

        protected override TabItem<SetupScreenMode> CreateTabItem(SetupScreenMode value) => new TabItem(value);

        private class TabItem : OsuTabItem
        {
            private const float transition_length = 250;

            public TabItem(SetupScreenMode value)
                : base(value)
            {
                Text.Margin = new MarginPadding();
                Text.Anchor = Anchor.CentreLeft;
                Text.Origin = Anchor.CentreLeft;
                // Change to light for deselected items if stated by flyte, currently not easy to tell from the drafts
                Text.Font = @"Exo2.0-Bold";
                Text.TextSize = 16;
            }

            protected override void OnActivated()
            {
                base.OnActivated();
                //Text.Font = @"Exo2.0-Bold";
                Bar.ScaleTo(new Vector2(1, 5), transition_length, Easing.OutQuint);
            }

            protected override void OnDeactivated()
            {
                base.OnDeactivated();
                //Text.Font = @"Exo2.0-Light";
                Bar.ScaleTo(Vector2.One, transition_length, Easing.OutQuint);
            }
        }
    }
}
