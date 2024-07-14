// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens.Editors.Components
{
    public partial class DeleteTeamDialog : DangerousActionDialog
    {
        public DeleteTeamDialog(TournamentTeam team, Action action)
        {
            HeaderText = team.FullName.Value.Length > 0 ? $@"Delete team ""{team.FullName.Value}""?" :
                team.Acronym.Value.Length > 0 ? $@"Delete team ""{team.Acronym.Value}""?" :
                @"Delete unnamed team?";
            Icon = FontAwesome.Solid.Trash;
            DangerousAction = action;
        }
    }
}
