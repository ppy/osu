// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public partial class ConfirmExitMultiplayerMatchDialog : ConfirmDialog
    {
        public ConfirmExitMultiplayerMatchDialog(Action onConfirm)
            : base(DialogStrings.ConfirmExitMultiplayerMatchBodyText, onConfirm)
        {
        }
    }
}
