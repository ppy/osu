// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Edit.Screens;
using OpenTK;

namespace osu.Game.Screens.Edit.Menus
{
    public class ScreenSelectionTabControl : OsuTabControl<EditorScreenMode>
    {
        public ScreenSelectionTabControl()
        {
            AutoSizeAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;

            TabContainer.RelativeSizeAxes &= ~Axes.X;
            TabContainer.AutoSizeAxes = Axes.X;
            TabContainer.Padding = new MarginPadding();

            Add(new Box
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                RelativeSizeAxes = Axes.X,
                Height = 1,
                Colour = Color4.White.Opacity(0.2f),
            });

            Current.Value = EditorScreenMode.Compose;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.Yellow;
        }

        protected override Dropdown<EditorScreenMode> CreateDropdown() => null;

        protected override TabItem<EditorScreenMode> CreateTabItem(EditorScreenMode value) => new TabItem(value);

        private class TabItem : OsuTabItem
        {
            private const float transition_length = 250;

            public TabItem(EditorScreenMode value)
                : base(value)
            {
                Text.Margin = new MarginPadding();
                Text.Anchor = Anchor.CentreLeft;
                Text.Origin = Anchor.CentreLeft;
            }

            protected override void OnActivated()
            {
                base.OnActivated();
                Bar.ScaleTo(new Vector2(1, 5), transition_length, Easing.OutQuint);
            }

            protected override void OnDeactivated()
            {
                base.OnDeactivated();
                Bar.ScaleTo(Vector2.One, transition_length, Easing.OutQuint);
            }
        }
    }
}
