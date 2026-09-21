// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Events;

namespace osu.Game.Screens.Select
{
    /// <summary>
    /// Handles mouse interactions required when moving away from the carousel.
    /// </summary>
    internal partial class LeftSideInteractionContainer : Container
    {
        private readonly Action? resetCarouselPosition;

        private bool mouseContained;

        private InputManager inputManager = null!;

        public LeftSideInteractionContainer(Action resetCarouselPosition)
        {
            this.resetCarouselPosition = resetCarouselPosition;
        }

        // we want to block plain scrolls on the left side so that they don't scroll the carousel,
        // but also we *don't* want to handle scrolls when they're combined with keyboard modifiers
        // as those will usually correspond to other interactions like adjusting volume.
        protected override bool OnScroll(ScrollEvent e) => !e.ControlPressed && !e.AltPressed && !e.ShiftPressed && !e.SuperPressed;

        protected override bool OnMouseDown(MouseDownEvent e) => true;

        protected override void LoadComplete()
        {
            inputManager = GetContainingInputManager()!;
            base.LoadComplete();
        }

        protected override void Update()
        {
            base.Update();

            // We want to trigger an action whenever the cursor is in the left area of song select.
            // Other elements in song select handle input, so rather than using `OnHover` let's check the true mouse position.
            if (Contains(inputManager.CurrentState.Mouse.Position))
            {
                if (!mouseContained)
                {
                    mouseContained = true;
                    resetCarouselPosition?.Invoke();
                }
            }
            else
            {
                mouseContained = false;
            }
        }
    }
}
