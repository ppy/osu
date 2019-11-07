// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public class DrawableToggleMenuItem : DrawableOsuMenuItem
    {
        protected new ToggleMenuItem Item => (ToggleMenuItem)base.Item;

        public DrawableToggleMenuItem(ToggleMenuItem item)
            : base(item)
        {
        }

        protected override TextContainer CreateTextContainer() => new ToggleTextContainer
        {
            State = { BindTarget = Item.State }
        };

        private class ToggleTextContainer : TextContainer
        {
            public readonly Bindable<bool> State = new Bindable<bool>();

            private readonly SpriteIcon checkmark;

            public ToggleTextContainer()
            {
                Add(checkmark = new SpriteIcon
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Icon = FontAwesome.Solid.Check,
                    Size = new Vector2(10),
                    Margin = new MarginPadding { Horizontal = MARGIN_HORIZONTAL },
                    AlwaysPresent = true,
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                State.BindValueChanged(state => checkmark.Alpha = state.NewValue ? 1 : 0, true);
            }

            protected override void Update()
            {
                base.Update();

                // Todo: This is bad. This can maybe be done better with a refactor of DrawableOsuMenuItem.
                checkmark.X = BoldText.DrawWidth + 10;
            }
        }
    }
}
