// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens.Editors.Components
{
    public partial class DeleteRefereeDialog : DangerousActionDialog
    {
        public DeleteRefereeDialog(TournamentUser user, Action action)
        {
            HeaderText = (user.Username.Length > 0) ? $@"Remove referee ""{user.Username}"" from this round?" : @"Remove the referee from this team?";
            Icon = FontAwesome.Solid.Trash;
            DangerousAction = action;
        }
    }
}
