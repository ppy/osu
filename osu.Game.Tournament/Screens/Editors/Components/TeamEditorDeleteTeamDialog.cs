// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Tournament.Screens.Editors.Components
{
    public partial class TeamEditorDeleteTeamDialog : DangerousActionDialog
    {
        public TeamEditorDeleteTeamDialog(string fullTeamName, string acronym, Action action)
        {
            HeaderText = fullTeamName.Length > 0 ? $@"Delete team ""{fullTeamName}""?" :
                acronym.Length > 0 ? $@"Delete team ""{acronym}""?" :
                @"Delete unnamed team?";
            Icon = FontAwesome.Solid.Trash;
            DangerousAction = action;
        }
    }
}
