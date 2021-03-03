// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Menu
{
    public class ConfirmExitDialog : ConfirmDialog
    {
        public ConfirmExitDialog(Action confirm, Action onCancel = null)
            : base("exit osu!", confirm, onCancel)
        {
            ButtonConfirm.Text = "Let me out!";
            ButtonCancel.Text = "Just a little more...";
        }
    }
}
