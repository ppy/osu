// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens.Editors.Components
{
    public partial class DeleteRoundDialog : DangerousActionDialog
    {
        public DeleteRoundDialog(TournamentRound round, Action action)
        {
            HeaderText = round.Name.Value.Length > 0 ? $@"Delete round ""{round.Name.Value}""?" : @"Delete unnamed round?";
            Icon = FontAwesome.Solid.Trash;
            DangerousAction = action;
        }
    }
}
