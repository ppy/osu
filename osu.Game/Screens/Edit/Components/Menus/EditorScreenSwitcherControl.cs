// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Components.Menus
{
    public class EditorScreenSwitcherControl : OsuTabControl<EditorScreenMode>
    {
        public EditorScreenSwitcherControl()
        {
            AutoSizeAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;

            TabContainer.RelativeSizeAxes &= ~Axes.X;
            TabContainer.AutoSizeAxes = Axes.X;
            TabContainer.Padding = new MarginPadding(10);
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            AccentColour = colourProvider.Light3;

            AddInternal(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = colourProvider.Background2,
            });
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

                Text.Font = OsuFont.TorusAlternate;

                Bar.Expire();
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
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
