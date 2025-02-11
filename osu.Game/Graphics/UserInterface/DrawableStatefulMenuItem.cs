// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Input;

namespace osu.Game.Graphics.UserInterface
{
    public partial class DrawableStatefulMenuItem : DrawableOsuMenuItem
    {
        protected new StatefulMenuItem Item => (StatefulMenuItem)base.Item;

        public DrawableStatefulMenuItem(StatefulMenuItem item)
            : base(item)
        {
        }

        protected override TextContainer CreateTextContainer() => new ToggleTextContainer(Item);

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            // Right mouse button is a special case where we allow actioning without dismissing the menu.
            // This is achieved by not calling `Clicked` (as done by the base implementation in OnClick).
            if (IsActionable && e.Button == MouseButton.Right)
            {
                Item.Action.Value?.Invoke();
                return true;
            }

            return false;
        }

        private partial class ToggleTextContainer : TextContainer
        {
            private readonly StatefulMenuItem menuItem;
            private readonly Bindable<object> state;
            private readonly SpriteIcon stateIcon;

            public ToggleTextContainer(StatefulMenuItem menuItem)
            {
                this.menuItem = menuItem;

                state = menuItem.State.GetBoundCopy();

                CheckboxContainer.Add(stateIcon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(10),
                    AlwaysPresent = true,
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                state.BindValueChanged(updateState, true);
            }

            private void updateState(ValueChangedEvent<object> state)
            {
                var icon = menuItem.GetIconForState(state.NewValue);

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
