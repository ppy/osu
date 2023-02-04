// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Screens.Edit.Setup
{
    internal abstract partial class LabelledTextBoxWithPopover : LabelledTextBox, IHasPopover
    {
        public abstract Popover GetPopover();

        protected override OsuTextBox CreateTextBox() =>
            new PopoverTextBox
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                CornerRadius = CORNER_RADIUS,
                OnFocused = this.ShowPopover
            };

        internal partial class PopoverTextBox : OsuTextBox
        {
            public Action? OnFocused;

            protected override bool OnDragStart(DragStartEvent e)
            {
                // This text box is intended to be "read only" without actually specifying that.
                // As such we don't want to allow the user to select its content with a drag.
                return false;
            }

            protected override void OnFocus(FocusEvent e)
            {
                if (Current.Disabled)
                    return;

                OnFocused?.Invoke();
                base.OnFocus(e);

                GetContainingInputManager().TriggerFocusContention(this);
            }
        }
    }
}
