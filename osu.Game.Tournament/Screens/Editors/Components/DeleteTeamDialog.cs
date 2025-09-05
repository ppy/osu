// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Overlays.Dialog;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens.Editors.Components
{
    public partial class DeleteTeamDialog : DeletionDialog
    {
        public DeleteTeamDialog(TournamentTeam team, Action action)
        {
            HeaderText = team.FullName.Value.Length > 0 ? DialogStrings.DeleteTeamPrompt(team.FullName.Value) :
                team.Acronym.Value.Length > 0 ? DialogStrings.DeleteTeamPrompt(team.Acronym.Value) :
                DialogStrings.DeleteUnnamedTeamPrompt;
            DangerousAction = action;
        }
    }
}
