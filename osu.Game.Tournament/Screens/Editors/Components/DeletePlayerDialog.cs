// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens.Editors.Components
{
    public partial class DeletePlayerDialog : DangerousActionDialog
    {
        public DeletePlayerDialog(TournamentUser user, Action action)
        {
            HeaderText = (user.Username.Length > 0) ? $@"Remove player ""{user.Username}"" from this team?" : @"Remove the player from this team?";
            Icon = FontAwesome.Solid.Trash;
            DangerousAction = action;
        }
    }
}
