// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public partial class ConfirmAbortDialog : DangerousActionDialog
    {
        public ConfirmAbortDialog()
        {
            HeaderText = "Are you sure you want to abort the match?";
        }
    }
}
