// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Game.Input.Bindings;

namespace osu.Game.Graphics.UserInterface
{
    public class BackButton : VisibilityContainer
    {
        public Action Action;

        private readonly TwoLayerButton button;

        public BackButton(Receptor receptor)
        {
            receptor.OnBackPressed = () => button.Click();

            Size = TwoLayerButton.SIZE_EXTENDED;

            Child = button = new TwoLayerButton
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                Text = @"back",
                Icon = OsuIcon.LeftCircle,
                Action = () => Action?.Invoke()
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            button.BackgroundColour = colours.Pink;
            button.HoverColour = colours.PinkDark;
        }

        protected override void PopIn()
        {
            button.MoveToX(0, 400, Easing.OutQuint);
            button.FadeIn(150, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            button.MoveToX(-TwoLayerButton.SIZE_EXTENDED.X / 2, 400, Easing.OutQuint);
            button.FadeOut(400, Easing.OutQuint);
        }

        public class Receptor : Drawable, IKeyBindingHandler<GlobalAction>
        {
            public Action OnBackPressed;

            public bool OnPressed(GlobalAction action)
            {
                switch (action)
                {
                    case GlobalAction.Back:
                        OnBackPressed?.Invoke();
                        return true;
                }

                return false;
            }

            public bool OnReleased(GlobalAction action) => action == GlobalAction.Back;
        }
    }
}
