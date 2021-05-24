// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public class DrawableStatefulMenuItem : DrawableOsuMenuItem
    {
        protected new StatefulMenuItem Item => (StatefulMenuItem)base.Item;

        public DrawableStatefulMenuItem(StatefulMenuItem item)
            : base(item)
        {
        }

        protected override TextContainer CreateTextContainer() => new ToggleTextContainer(Item, CloseMenuOnClick);

        private class ToggleTextContainer : TextContainer
        {
            private readonly StatefulMenuItem menuItem;
            private readonly Bindable<object> state;
            private readonly SpriteIcon stateIcon;
            private readonly bool closeMenuOnClick;

            public ToggleTextContainer(StatefulMenuItem menuItem, bool closeMenuOnClick)
            {
                this.menuItem = menuItem;
                this.closeMenuOnClick = closeMenuOnClick;

                state = menuItem.State.GetBoundCopy();

                Add(stateIcon = new SpriteIcon
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Size = new Vector2(10),
                    Margin = new MarginPadding { Horizontal = MARGIN_HORIZONTAL },
                    AlwaysPresent = true,
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                updateState();

                if (!closeMenuOnClick)
                    state.BindValueChanged(_ => updateState());
            }

            protected override void Update()
            {
                base.Update();

                // Todo: This is bad. This can maybe be done better with a refactor of DrawableOsuMenuItem.
                stateIcon.X = BoldText.DrawWidth + 10;
            }

            private void updateState()
            {
                var icon = menuItem.GetIconForState(state.Value);

                if (icon == null)
                    stateIcon.Alpha = 0;
                else
                {
                    stateIcon.Alpha = 1;
                    stateIcon.Icon = icon.Value;
                }
            }
        }
    }
}
